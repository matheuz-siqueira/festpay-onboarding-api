# 🧪 Teste Técnico - Dev Fullstack (.NET/C#) - Festpay

## 🎯 Objetivo

Construir e manter uma api em .NET 9 utilizando o padrão CQRS afim de manter um sistema de contas e transações da Festpay. Utilizando dos métodos já existentes, construa a entidade de Transações e o seu respectivo CRUD.
A entidade deve herdar a entidade base e possuir os seguintes dados:

- **Conta de destino**
- **Conta de origem**
- **Valor**
- **Cancelada**

Deverá ser desenvolvido métodos para:

- **Buscar todas as transações**
- **Buscar uma transação pelo Id**
- **Inserir uma transação**
- **Cancelar uma transação**

---

**ATENÇÃO** - Não se esqueça de desenvolver os testes de domínio e testes de aplicação.

---

## 🧱 Critérios de Avaliação

- Separação das regras de domínio e regras de aplicação
- Estrutura e funcionalidade do código existente e do código redigido
- Uso correto da arquitetura definida no projeto
- Princípios SOLID
- Tratamento de exceções
- Código limpo e organizado

---

## 📤 Entrega

- Criar um fork do projeto e submetê-lo com as implementações
- Atualizar o README com:
  - Tecnologias utilizadas
  - Instruções para rodar o projeto
- As instruções para envio do projeto deverão seguir as orientações enviadas pelo recrutador.

## Tecnologias Utilizadas: 
* SQLite 
* .NET 9 
* MediatR 
* FluentValidation
* Swagger

## Instruções para rodar o projeto 


Buildar a aplicação com o comando: 

``dotnet build``

Para aplicar as migrations no projeto **Festpay.Onboarding.Infra** rodar o comando: 

``dotnet ef database update`` 

Para rodar a aplicação pelo terminal acessar o projeto **Festpay.Onboarding.Api** e utilizar o comando: 

``dotnet watch run`` 
