# 🔐 Sistema de Autenticação com .NET 8 e JWT

Este é um projeto de **API RESTful** construído com **.NET 8** que fornece uma base robusta para autenticação e registro de usuários.
A arquitetura foi projetada para ser **limpa, escalável e segura**, utilizando uma camada de serviço para a lógica de negócio e **Entity Framework Core** para a persistência de dados.

---

## ✨ Funcionalidades Principais

* **Registro de Usuários**: Endpoint para criar novos usuários com validação de dados e armazenamento seguro de senhas.
* **Autenticação de Usuários**: Endpoint de login que retorna um **JSON Web Token (JWT)** em caso de sucesso.
* **Segurança de Senhas**: As senhas são hasheadas usando o algoritmo **BCrypt**, garantindo que nunca sejam armazenadas em texto plano.
* **Gerenciamento de Sessão Stateless**: Utiliza **tokens JWT** para autenticar requisições, ideal para arquiteturas de microsserviços e clientes desacoplados.
* **Documentação Interativa**: Integração com o **Swagger (OpenAPI)** para documentar e testar os endpoints da API diretamente pelo navegador.
* **Arquitetura Limpa**: Separação de responsabilidades entre **Controllers (API)**, **Services (Lógica de Negócio)** e **Data (Acesso a Dados)**.

---

## 🛠️ Tecnologias Utilizadas

* **Framework**: .NET 8
* **Linguagem**: C#
* **Banco de Dados**: Microsoft SQL Server (configurado com Entity Framework Core 8)
* **Autenticação**: JSON Web Tokens (JWT)
* **Hashing de Senha**: BCrypt.Net-Next
* **Documentação da API**: Swashbuckle (Swagger)
* **ORM**: Entity Framework Core 8

---

## 📋 Pré-requisitos

Antes de começar, você precisará ter as seguintes ferramentas instaladas em sua máquina:

* [.NET 8 SDK](https://dotnet.microsoft.com/)
* [Microsoft SQL Server](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) (edição **Developer** recomendada para desenvolvimento)
* [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup) (ou outra ferramenta de gerenciamento)
* [Git](https://git-scm.com/) (opcional, mas recomendado)

---

## 🚀 Como Rodar o Projeto

### 1. Clone o Repositório

```bash
git clone https://seu-repositorio-aqui.git
cd AuthSystem
```

### 2. Configure a String de Conexão

No arquivo `appsettings.json`, ajuste a **DefaultConnection** para apontar para a sua instância do SQL Server (a configuração padrão usa Autenticação do Windows):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=AuthDb;Trusted_Connection=True;TrustServerCertificate=true;"
}
```

### 3. Instale as Ferramentas do EF Core

```bash
dotnet tool install --global dotnet-ef
```

### 4. Aplique as Migrações

Este comando criará o banco de dados `AuthDb` e a tabela `Users`:

```bash
dotnet ef database update
```

### 5. Execute a Aplicação

```bash
dotnet run
```

A API estará disponível em:

* `http://localhost:5081`
* `https://localhost:7234`

### 6. Teste com o Swagger

Acesse no navegador:

```
https://localhost:7234/swagger
```

---

## 📂 Estrutura do Projeto

```
AuthSystem/
├── Controllers/     # Recebem requisições HTTP e retornam respostas
├── Data/            # Contém o DbContext do Entity Framework
├── DTOs/            # Data Transfer Objects (dados entre cliente e servidor)
├── Migrations/      # Arquivos de migração do EF Core
├── Models/          # Entidades do domínio (ex: User)
├── Services/        # Lógica de negócio da aplicação
├── appsettings.json # Configurações
└── Program.cs       # Ponto de entrada da aplicação
```

---

## 📌 Endpoints da API

### 🔑 Autenticação

#### **Registrar Usuário**

`POST /api/auth/register`

**Body (JSON):**

```json
{
  "username": "seu_usuario",
  "email": "seu_email@exemplo.com",
  "password": "sua_senha_forte"
}
```

#### **Login**

`POST /api/auth/login`

**Body (JSON):**

```json
{
  "usernameOrEmail": "seu_usuario_ou_email",
  "password": "sua_senha"
}
```

---
