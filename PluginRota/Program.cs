using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk.Client;
using System.Linq;

namespace PluginRota
{
    public class ValidarCPFPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Faz a busca do contexto do serviço da organização
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se o gatilho é do tipo "create" e se a tabela é do tipo "smt_cliente"
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_cliente")
            {
                // Verifica se o contexto do formulário e dos campos
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetCliente = (Entity)context.InputParameters["Target"];
                    string cpf = string.Empty;

                    // Verifica se no formulário existe o CPF
                    if (targetCliente.Contains("smt_cpf"))
                    {
                        // Armazena o valor do campo CPF
                        cpf = targetCliente["smt_cpf"].ToString();

                        // Faz a requisição passando a variável com o valor do CPF
                        string xml = string.Format(
                            @"<fetch version='1.0' output-format='xmlplatform' mapping='logical' distinct='true'>
                        <entity name='smt_cliente'>
                            <attribute name='smt_cpf' />
                            <filter type='and'>
                                <condition attribute='smt_cpf' operator='eq' value='{0}'/>
                            </filter>
                        </entity>
                      </fetch>",
                            cpf);

                        EntityCollection resultado = service.RetrieveMultiple(new FetchExpression(xml));

                        // Caso já exista um CPF cadastrado, retorna um erro
                        if (resultado.Entities.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Este CPF já está cadastrado.");
                        }
                    }
                }
            }
        }
    }

    public class ValidarMarcaPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Faz a busca do contexto do serviço da organização
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se o gatilho é do tipo "create" e se a tabela é do tipo "smt_marca"
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_marca")
            {
                // Verifica se o contexto do formulário e dos campos
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    Entity targetMarca = (Entity)context.InputParameters["Target"];
                    string nomeMarca = string.Empty;

                    // Verifica se no formulário existe o campo nome da marca
                    if (targetMarca.Contains("smt_nomemarca"))
                    {
                        // Armazena o valor do campo nome da marca
                        nomeMarca = targetMarca["smt_nomemarca"].ToString();

                        // Faz a requisição passando a variável com o valor do nome da marca
                        string xml = string.Format(
                            @"<fetch version='1.0' output-format='xmlplatform' mapping='logical' distinct='true'>
                        <entity name='smt_marca'>
                            <attribute name='smt_nomemarca' />
                            <filter type='and'>
                                <condition attribute='smt_nomemarca' operator='eq' value='{0}'/>
                            </filter>
                        </entity>
                      </fetch>",
                            nomeMarca);

                        EntityCollection resultado = service.RetrieveMultiple(new FetchExpression(xml));

                        // Caso já exista uma marca cadastrada, retorna um erro
                        if (resultado.Entities.Count > 0)
                        {
                            throw new InvalidPluginExecutionException("Esta marca já está cadastrada.");
                        }
                    }
                }
            }
        }

    }

    public class VerificaPendenciaCliente : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obter o contexto do serviço da organização
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verificar se o evento é de criação e se a entidade é "smt_controledeacesso"
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName == "smt_controledeacesso")
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target)
                {
                    // Obter o ID do cliente a partir do registro "Controle de Acesso"
                    if (target.Contains("smt_cliente"))
                    {
                        EntityReference clienteRef = target.GetAttributeValue<EntityReference>("smt_cliente");
                        if (clienteRef != null)
                        {
                            Guid clienteId = clienteRef.Id;

                            // Recuperar os dados do cliente para verificar se possui pendências
                            Entity cliente = service.Retrieve("smt_cliente", clienteId, new ColumnSet("smt_possuipendencias"));

                            // Verificar se o cliente possui pendências
                            if (cliente.Contains("smt_possuipendencias") && cliente.GetAttributeValue<bool>("smt_possuipendencias"))
                            {
                                throw new InvalidPluginExecutionException("O cliente possui pendências e não pode usar o estacionamento.");
                            }
                        }
                    }
                }
            }
        }
    }

    public class PlacaMercosulValidator : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtém o contexto da execução
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity entity)
            {
                // Verifica se é a entidade correta (Tabela Veículos)
                if (entity.LogicalName != "smt_veiculo") return;

                // Verifica se o campo da placa está presente
                if (entity.Contains("smt_placa"))
                {
                    string placa = entity["smt_placa"].ToString().ToUpper();

                    // Regex para placas Mercosul com ou sem hífen
                    Regex mercosulPattern = new Regex(@"^[A-Z]{3}-?\d[A-Z]\d{2}$");

                    // Define o campo smt_placamercosul como verdadeiro ou falso
                    entity["smt_placamercosul"] = mercosulPattern.IsMatch(placa);
                }
            }
        }
    }
    public class ValidarEntradaVeiculo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity acesso = (Entity)context.InputParameters["Target"];

                // Verifica se a entidade é "smt_controledeacesso"
                if (acesso.LogicalName != "smt_controledeacesso")
                    return;

                // Obtém o veículo associado ao acesso
                if (!acesso.Contains("smt_veiculo"))
                    return;

                EntityReference veiculoRef = (EntityReference)acesso["smt_veiculo"];
                Guid veiculoId = veiculoRef.Id;

                // Consulta acessos existentes para este veículo sem data de saída
                QueryExpression query = new QueryExpression("smt_controledeacesso")
                {
                    ColumnSet = new ColumnSet("smt_veiculo", "smt_datadesaida"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                    {
                        new ConditionExpression("smt_veiculo", ConditionOperator.Equal, veiculoId),
                        new ConditionExpression("smt_datadesaida", ConditionOperator.Null) // Sem data de saída
                    }
                    }
                };

                EntityCollection result = service.RetrieveMultiple(query);

                if (result.Entities.Count > 0)
                {
                    throw new InvalidPluginExecutionException("Este veículo ainda não saiu do estacionamento. Não é possível criar um novo acesso.");
                }
            }
        }
    }
    public class AplicarDescontoEspecial : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtém o contexto de execução
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Verifica se é uma operação de criação ou atualização na tabela smt_controledeacesso
            if (context.PrimaryEntityName == "smt_controledeacesso" &&
                context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verifica se a entidade possui os atributos necessários
                if (entity.Contains("smt_valorespecial") && entity.Contains("smt_valortotal_"))
                {
                    Money smtValorEspecial = entity.GetAttributeValue<Money>("smt_valorespecial");
                    Money smtValorTotal_ = entity.GetAttributeValue<Money>("smt_valortotal_");

                    // Verifica se smt_valorespecial possui um valor
                    if (smtValorEspecial != null)
                    {
                        // Atualiza smt_valortotal com o valor de smt_valorespecial
                        entity["smt_valortotal"] = smtValorEspecial;
                    }
                    else if (smtValorTotal_ != null)
                    {
                        // Atualiza smt_valortotal com o valor de smt_valortotal_
                        entity["smt_valortotal"] = smtValorTotal_;
                    }
                }
            }
        }
    }
}


