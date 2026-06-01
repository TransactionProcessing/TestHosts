using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework;
using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleResults;
using TestHosts.AgencyBanking.Database;
using TestHosts.AgencyBanking.Database.Entities;
using TestHosts.AgencyBanking.Services;
using TestHosts.DataTransferObjects.AgencyBanking;
using TestHosts.PataPawa.Database;
using TestHosts.PataPawa.Handlers;

namespace TestHosts.AgencyBanking.Endpoints
{
    public static class AgencyBankingEndpoints
    {
        public static WebApplication MapAgencyBankingSystemSetupEndpoints(this WebApplication app) {
            app.MapPost("/api/agencybanking/system/initialize", async (SystemInitializationRequest request,
                                                         AgencyBankingDbContext db) => {
                var exists = await db.SystemConfigurations.AnyAsync();

                if (exists) {
                    return Results.BadRequest("System already initialized");
                }

                var config = new SystemConfiguration {
                    InstitutionCode = request.InstitutionCode,
                    InstitutionName = request.InstitutionName,
                    DefaultCurrency = request.DefaultCurrency,
                    Timezone = request.Timezone,
                    SettlementMode = request.SettlementMode,
                    InitializedAt = DateTime.UtcNow
                };

                db.SystemConfigurations.Add(config);

                await db.SaveChangesAsync();

                return Results.Ok(new { responseCode = "00", responseMessage = "System Initialized" });
            });

            app.MapPost("/api/agencybanking/glaccounts", async (CreateGlAccountRequest request,
                                                  AgencyBankingDbContext db) => {
                var exists = await db.GlAccounts.AnyAsync(x => x.GlCode == request.GlCode);

                if (exists) {
                    return Results.BadRequest("GL account already exists");
                }

                var gl = new GlAccount {
                    GlCode = request.GlCode,
                    GlName = request.GlName,
                    GlType = request.GlType,
                    Currency = request.Currency,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.GlAccounts.Add(gl);

                await db.SaveChangesAsync();

                return Results.Ok(gl);
            });
            
            app.MapPost("/api/agencybanking/settlement/accounts", async (CreateSettlementAccountRequest request,
                                                           AgencyBankingDbContext db) => {
                var account = new SettlementAccount {
                    AccountNumber = request.AccountNumber,
                    BankCode = request.BankCode,
                    Currency = request.Currency,
                    AccountName = request.AccountName,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.SettlementAccounts.Add(account);

                await db.SaveChangesAsync();

                return Results.Ok(account);
            });

            app.MapPost("/api/agencybanking/agents/super-agent", async (CreateSuperAgentRequest request,
                                                          AgencyBankingDbContext db) => {
                var exists = await db.SuperAgents.AnyAsync(x => x.AgentId == request.AgentId);

                if (exists) {
                    return Results.BadRequest("Super agent exists");
                }

                var agent = new SuperAgent {
                    AgentId = request.AgentId,
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Region = request.Region,
                    DailyLimit = request.DailyLimit,
                    MinimumFloat = request.MinimumFloat,
                    Active = false,
                    CreatedAt = DateTime.UtcNow
                };

                db.SuperAgents.Add(agent);

                await db.SaveChangesAsync();

                return Results.Ok(agent);
            });

            app.MapPost("/api/agencybanking/agents", async (CreateAgentRequest request,
                                              AgencyBankingDbContext db) => {
                var exists = await db.Agents.AnyAsync(x => x.AgentId == request.AgentId);

                if (exists) {
                    return Results.BadRequest("Agent exists");
                }

                var agent = new Agent {
                    AgentId = request.AgentId,
                    SuperAgentId = request.SuperAgentId,
                    Name = request.Name,
                    PhoneNumber = request.PhoneNumber,
                    Email = request.Email,
                    Location = request.Location,
                    DailyLimit = request.DailyLimit,
                    MinimumFloat = request.MinimumFloat,
                    FloatBalance = 0,
                    Active = false,
                    CreatedAt = DateTime.UtcNow
                };

                db.Agents.Add(agent);

                await db.SaveChangesAsync();

                return Results.Ok(agent);
            });

            app.MapPost("/api/agencybanking/agents/{agentId}/activate", async (string agentId,
                                                                 ActivateAgentRequest request,
                                                                 AgencyBankingDbContext db) => {
                var agent = await db.Agents.FirstOrDefaultAsync(x => x.AgentId == agentId);

                if (agent == null) {
                    return Results.NotFound();
                }

                agent.Active = true;

                agent.ActivatedAt = DateTime.UtcNow;

                agent.ActivatedBy = request.ActivatedBy;

                await db.SaveChangesAsync();

                return Results.Ok(new { responseCode = "00", responseMessage = "Agent Activated" });
            });

            app.MapPost("/api/agencybanking/float/configure", async (ConfigureFloatRequest request,
                                                       AgencyBankingDbContext db) => {
                var config = await db.FloatConfigurations.FirstOrDefaultAsync(x => x.AgentId == request.AgentId);

                if (config == null) {
                    config = new FloatConfiguration { AgentId = request.AgentId };

                    db.FloatConfigurations.Add(config);
                }

                config.MinimumFloat = request.MinimumFloat;

                config.MaximumFloat = request.MaximumFloat;

                config.DailyFloatLimit = request.DailyFloatLimit;

                config.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync();

                return Results.Ok(config);
            });


            app.MapPost("/api/agencybanking/float/credit", FloatHandlers.CreditFloat);

            app.MapPost("/api/agencybanking/customers", async (CreateCustomerRequest request,
                                                 AgencyBankingDbContext db) => {
                var exists = await db.Customers.AnyAsync(x => x.CustomerId == request.CustomerId);

                if (exists) {
                    return Results.BadRequest("Customer exists");
                }

                var customer = new Customer {
                    CustomerId = request.CustomerId,
                    FullName = request.FullName,
                    PhoneNumber = request.PhoneNumber,
                    NationalId = request.NationalId,
                    AccountNumber = request.AccountNumber,
                    CreatedAt = DateTime.UtcNow
                };

                db.Customers.Add(customer);

                // Create Account
                db.Accounts.Add(new Account {
                    AccountNumber = request.AccountNumber,
                    CustomerId = request.CustomerId,
                    Balance = 0,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                });

                await db.SaveChangesAsync();

                return Results.Ok(customer);
            });
            
            app.MapPost("/api/agencybanking/fees", async (FeeConfigurationRequest request,
                                            AgencyBankingDbContext db) => {
                var fee = new FeeConfiguration {
                    TransactionType = request.TransactionType,
                    MinimumAmount = request.MinimumAmount,
                    MaximumAmount = request.MaximumAmount,
                    FeeAmount = request.FeeAmount,
                    CreatedAt = DateTime.UtcNow
                };

                db.FeeConfigurations.Add(fee);

                await db.SaveChangesAsync();

                return Results.Ok(fee);
            });

            app.MapPost("/api/agencybanking/commissions", async (CommissionConfigurationRequest request,
                                                   AgencyBankingDbContext db) => {
                var commission = new CommissionConfiguration { TransactionType = request.TransactionType, CommissionType = request.CommissionType, CommissionValue = request.CommissionValue, CreatedAt = DateTime.UtcNow };

                db.CommissionConfigurations.Add(commission);

                await db.SaveChangesAsync();

                return Results.Ok(commission);
            });

            app.MapPost("/api/agencybanking/settlement/windows", async (SettlementWindowRequest request,
                                                          AgencyBankingDbContext db) => {
                var window = new SettlementWindow {
                    WindowName = request.WindowName,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    SettlementMode = request.SettlementMode,
                    CreatedAt = DateTime.UtcNow
                };

                db.SettlementWindows.Add(window);

                await db.SaveChangesAsync();

                return Results.Ok(window);
            });

            app.MapPost("/api/agencybanking/channels", async (ChannelRequest request,
                                                AgencyBankingDbContext db) => {
                var channel = new Channel { ChannelCode = request.ChannelCode, ChannelName = request.ChannelName, Enabled = request.Enabled, CreatedAt = DateTime.UtcNow };

                db.Channels.Add(channel);

                await db.SaveChangesAsync();

                return Results.Ok(channel);
            });
            
            app.MapPost("/api/agencybanking/clients", async (ApiClientRequest request,
                                               AgencyBankingDbContext db) => {
                var client = new ApiClient {
                    ClientId = request.ClientId,
                    ClientName = request.ClientName,
                    AllowedIps = string.Join(",", request.AllowedIps),
                    Scopes = string.Join(",", request.Scopes),
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.ApiClients.Add(client);

                await db.SaveChangesAsync();

                return Results.Ok(client);
            });
            
            app.MapPost("/api/agencybanking/limits", async (LimitConfigurationRequest request,
                                              AgencyBankingDbContext db) => {
                var limit = new LimitConfiguration { TransactionType = request.TransactionType, PerTransactionLimit = request.PerTransactionLimit, DailyLimit = request.DailyLimit, CreatedAt = DateTime.UtcNow };

                db.LimitConfigurations.Add(limit);

                await db.SaveChangesAsync();

                return Results.Ok(limit);
            });
            
            app.MapPost("/api/agencybanking/compliance/aml-rules", async (AmlRuleRequest request,
                                                            AgencyBankingDbContext db) => {
                var rule = new AmlRule {
                    RuleCode = request.RuleCode,
                    TransactionType = request.TransactionType,
                    ThresholdAmount = request.ThresholdAmount,
                    Action = request.Action,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.AmlRules.Add(rule);

                await db.SaveChangesAsync();

                return Results.Ok(rule);
            });
            
            app.MapPost("/api/agencybanking/system/go-live", async (GoLiveRequest request,
                                                      AgencyBankingDbContext db) => {
                var goLive = new GoLiveRecord { ApprovedBy = request.ApprovedBy, Environment = request.Environment, GoLiveDate = DateTime.UtcNow };

                db.GoLiveRecords.Add(goLive);

                await db.SaveChangesAsync();

                return Results.Ok(new { responseCode = "00", responseMessage = "System Live" });
            });

            return app;
        }
    }

    public static class FloatHandlers
    {
        public static async Task<IResult> CreditFloat(
            FloatCreditRequest request,
            IFloatService floatService)
        {
            Result response = await floatService.CreditFloat(request.AgentId, request.Amount, request.TransactionId, request.Narration);

            if (response.IsFailed) {
                return Results.BadRequest(response);
            }

            return Results.Ok(response);
        }
    }
}
