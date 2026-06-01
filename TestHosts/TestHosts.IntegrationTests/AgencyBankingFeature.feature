@base @system @glaccount @float @settlement @agent @customer @transactions
Feature: Agency Banking Transactions	

Background:  
    Given the Agency Banking API is available

	When I initialize the system with:       
    | Field           | Value   |
      | institutionCode   | BANK001              |
      | institutionName   | Enterprise Bank      |
      | defaultCurrency   | KES                  |
      | timezone          | Africa/Nairobi       |
      | settlementMode    | NET                  |
    
    When I create a GL account with:
    | Field           | Value   |
        | code     |                 200100 |
        | name     | Agent Float GL         |
        | type     | LIABILITY              |
		| currency | KES                    |

When I create a GL account with:
| Field           | Value   |
        | code     |                 200200 |
        | name     | Settlement Suspense GL |
        | type     | LIABILITY              |
		| currency | KES                    |

        When I create a GL account with:
        | Field           | Value   |
        | code     |                 400100 |
        | name     | Agent Commission GL    |
        | type     | EXPENSE                |
		| currency | KES                    |

	When I create a GL account with:
    | Field           | Value   |
		| code     |      200300 |
		| name     | Reversal GL |
		| type     | LIABILITY   |
		| currency | KES         |

	When I create a GL account with:
    | Field           | Value   |
		| code     |        400200 |
		| name     | Fee Income GL |
		| type     | INCOME        |
		| currency | KES           |

    When I create a settlement account with:
    | Field           | Value   |
      | accountNumber | 999000001               |
      | bankCode      | 001                     |
      | currency      | KES                     |
      | accountName   | Main Settlement Account |
    
     When I create a super agent with:
     | Field           | Value   |
      | agentId       | SAG001              |
      | name          | Staging Estate      |
      | phoneNumber   | 254700000001        |
      | email         | superagent@bank.com |
      | region        | NAIROBI             |
      | dailyLimit    | 10000000            |
      | minimumFloat  | 500000              |
    
     When I create a retail agent with:
     | Field           | Value   |
      | agentId       | AGT001             |
      | superAgentId  | SAG001             |
      | name          | Staging Merchant 1 |
      | phoneNumber   | 254700000010       |
      | email         | agent@bank.com     |
      | location      | NAIROBI CBD        |
      | dailyLimit    | 100000             |
      | minimumFloat  | 10000              |

    When I activate agent "AGT001" with:
    | Field           | Value   |
      | activatedBy | SYSTEM_ADMIN |

    When I configure float for agent "AGT001" with:
    | Field           | Value   |
      | minimumFloat     | 10000   |
      | maximumFloat     | 500000  |
      | dailyFloatLimit  | 1000000 |

    When I credit float to agent "AGT001" with:
    | Field           | Value   |
      | transactionId | FLT000001             |
      | amount        |                 10000 |
      | sourceAccount | TREASURY_GL           |
      | narration     | Initial float funding |
    
    When I create a customer with:
    | Field           | Value   |
      | customerId    | CUST001        |
      | fullName      | John Doe       |
      | phoneNumber   | 254711111111   |
      | nationalId    | 12345678       |
      | accountNumber | 100200300      |

    When I approve go live with:
    | Field           | Value   |
      | approvedBy  | HEAD_OPERATIONS |
      | environment | UAT             |

@Deposit
  Scenario: Successful cash deposit transaction

    When agent "AGT001" performs a cash deposit with:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    Then the transaction id "DEP000001" should be successful and the transaction status should be "COMPLETED"    
    And the customer account "100200300" balance should be 5000
    And the agent "AGT001" float balance should be 5000

  @Withdrawal
  Scenario: Successful cash withdrawal transaction
    When agent "AGT001" performs a cash deposit with:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    And agent "AGT001" performs a cash withdrawal with:
      | Field           | Value   |
      | transactionId    | WDL000001                    |
      | customerId       | CUST001                      |
      | accountNumber    | 100200300                    |
      | amount           | 2000                         |
      | currency         | KES                          |
      | channel          | AGENCY                       |
      | narration        | Cash withdrawal via agent    |
      | referenceNumber  | REFWDL001                    |
    Then the transaction id "WDL000001" should be successful and the transaction status should be "COMPLETED"
    And the customer account "100200300" balance should be 3000
    And the agent "AGT001" float balance should be 7000

  @BalanceEnquiry
  Scenario: Successful customer balance enquiry
    When agent "AGT001" performs a cash deposit with:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    And agent "AGT001" performs a cash withdrawal with:
      | Field           | Value   |
      | transactionId    | WDL000001                    |
      | customerId       | CUST001                      |
      | accountNumber    | 100200300                    |
      | amount           | 2000                         |
      | currency         | KES                          |
      | channel          | AGENCY                       |
      | narration        | Cash withdrawal via agent    |
      | referenceNumber  | REFWDL001                    |
    Then the transaction id "WDL000001" should be successful and the transaction status should be "COMPLETED"
    When agent "AGT001" performs a balance enquiry with:
      | Field           | Value   |
      | transactionId    | BAL000001                 |
      | customerId       | CUST001                   |
      | accountNumber    | 100200300                 |
      | channel          | AGENCY                    |
      | referenceNumber  | REFBAL001                 |
    Then the available account balance for "100200300" should be returned as 3000

  @Reversal
  Scenario: Reverse a successful withdrawal transaction
    When agent "AGT001" performs a cash deposit with:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    And agent "AGT001" performs a cash withdrawal with:
      | Field           | Value   |
      | transactionId    | WDL000001                    |
      | customerId       | CUST001                      |
      | accountNumber    | 100200300                    |
      | amount           | 2000                         |
      | currency         | KES                          |
      | channel          | AGENCY                       |
      | narration        | Cash withdrawal via agent    |
      | referenceNumber  | REFWDL001                    |
    Then the transaction id "WDL000001" should be successful and the transaction status should be "COMPLETED"
    When agent "AGT001" performs a balance enquiry with:
      | Field           | Value   |
      | transactionId    | BAL000001                 |
      | customerId       | CUST001                   |
      | accountNumber    | 100200300                 |
      | channel          | AGENCY                    |
      | referenceNumber  | REFBAL001                 |
    Then the available account balance for "100200300" should be returned as 3000
    When agent "AGT001" performs a transaction reversal with:
      | Field           | Value   |
      | reversalTransactionId  | REV000001                        |
      | originalTransactionId  | WDL000001                        |
      | reversalReason         | Customer disputed withdrawal     |
      | initiatedBy            | SYSTEM_ADMIN                     |
      | channel                | AGENCY                           |
    Then the transaction id "REV000001" should be successful and the transaction status should be "COMPLETED"
    And the transaction id "WDL000001" should be successful and the transaction status should be "REVERSED"
    #And the customer account balance should be restored
    #And the agent float balance should be adjusted accordingly

  @FailedWithdrawal
  Scenario: Withdrawal should fail when agent float is insufficient
    When agent "AGT001" performs a cash withdrawal with which fails:
      | Field           | Value   |
      | transactionId    | WDL000002                  |
      | customerId       | CUST001                    |
      | accountNumber    | 100200300                  |
      | amount           | 1000000                    |
      | currency         | KES                        |
      | channel          | AGENCY                     |
      | narration        | Large withdrawal attempt   |
      | referenceNumber  | REFWDL002                  |
    Then the transaction id "WDL000002" should fail And the transaction status should be "FAILED"
    And the response code for transaction id "WDL000002" should be "InsufficientFunds"

  @FailedDeposit
  Scenario: Deposit should fail for invalid customer account
    When agent "AGT001" performs a cash deposit with which fails:
      | Field           | Value   |
      | transactionId    | DEP000002                |
      | customerId       | CUST999                  |
      | accountNumber    | 999999999                |
      | amount           | 3000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Invalid account deposit  |
      | referenceNumber  | REFDEP002                |
    Then the transaction id "DEP000002" should fail And the transaction status should be "FAILED"
    And the response code for transaction id "DEP000002" should be "InvalidCustomerAccount"

    @DuplicateTransaction
    Scenario: Duplicate transaction should be rejected
    When agent "AGT001" performs a cash deposit with:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    When agent "AGT001" performs a cash deposit with which fails:
      | Field           | Value   |
      | transactionId    | DEP000001                |
      | customerId       | CUST001                  |
      | accountNumber    | 100200300                |
      | amount           | 5000                     |
      | currency         | KES                      |
      | channel          | AGENCY                   |
      | narration        | Cash deposit via agent   |
      | referenceNumber  | REFDEP001                |
    Then the transaction id "DEP000001" should fail And the transaction status should be "FAILED"
    And the response code for transaction id "DEP000001" should be "DuplicateTransaction"

#
#  @DuplicateTransaction
#  Scenario: Duplicate transaction should be rejected
#    Given a transaction already exists with ID "DEP000001"
#    When agent "AGT001" performs a cash deposit with:
#      | Field           | Value   |
#      | transactionId    | DEP000001                |
#      | customerId       | CUST001                  |
#      | accountNumber    | 100200300                |
#      | amount           | 5000                     |
#      | currency         | KES                      |
#      | channel          | AGENCY                   |
#      | narration        | Duplicate deposit        |
#      | referenceNumber  | REFDEP003                |
#    Then the deposit transaction should fail
#    And the transaction status should be "FAILED"
#    And the error message should contain "Duplicate transaction"
