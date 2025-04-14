# 🔌 Plugin Rota X - Power Platform

Este projeto é um plugin desenvolvido para a **Power Platform (Dataverse)** com o objetivo de impedir o cadastro de registros duplicados na entidade de veículos, validando o nome antes do salvamento.

## 🚗 Sobre o Projeto

O **Plugin Rota X** foi criado para garantir a integridade dos dados em uma aplicação de gestão de veículos, impedindo que dois registros com o mesmo nome sejam salvos no Dataverse. A lógica é executada durante o evento de criação do registro, utilizando **C#**, **.NET Framework** e o **Plugin Registration Tool**.

## 💡 Funcionalidade Principal

- ❌ Impede o cadastro de veículos com o mesmo nome  
- ✅ Executa a validação na criação do registro (pipeline do Dataverse)
- 🧠 Desenvolvido com boas práticas de plugins na Power Platform

## 🛠️ Tecnologias Utilizadas

- C# (.NET Framework)
- Microsoft Dataverse
- Power Platform Plugin Registration Tool
- Visual Studio
- Xrm SDK

## ⚙️ Como Funciona

1. O plugin é registrado no evento **Create** da entidade de **Veículo**.
2. Ao tentar salvar um novo registro, o plugin verifica se já existe um com o mesmo nome.
3. Caso exista, a operação é cancelada com uma exceção personalizada.

## 📌 Observações

- Certifique-se de que a entidade de veículos esteja criada no ambiente da Power Platform.
- O plugin está preparado para exibir mensagens claras ao usuário em caso de duplicidade.
- Nome lógico do campo validado: smt_name.

## 🚀 Melhorias Futuras

- Implementar validação também para o campo de CPF (smt_cpf)
- Adicionar suporte ao evento de Update
- Criar testes unitários com mocks do Dataverse

##👩‍💻 Desenvolvedora
Projeto criado por Francine Maciel
