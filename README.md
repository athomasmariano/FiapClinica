# 🏥 FiapClinica API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-ASP.NET_Core-239120?style=for-the-badge&logo=csharp)
![Oracle](https://img.shields.io/badge/Oracle-Database-F80000?style=for-the-badge&logo=oracle)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-Mensageria-FF6600?style=for-the-badge&logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-Container-2496ED?style=for-the-badge&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

> API REST robusta para gerenciamento de pacientes com mensageria assíncrona, desenvolvida como projeto acadêmico na **FIAP**.

---

## 📋 Sumário

- [Sobre o Projeto](#-sobre-o-projeto)
- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Como Rodar](#-como-rodar)
- [Configuração](#️-configuração)
- [Endpoints](#-endpoints-da-api)
- [Fluxo de Mensageria](#-fluxo-de-mensageria)
- [Resiliência e Tratamento de Erros](#️-resiliência-e-tratamento-de-erros)

---

## 🩺 Sobre o Projeto

O **FiapClinica** é uma API REST desenvolvida em **C# com ASP.NET Core** que gerencia o cadastro de pacientes de uma clínica. O projeto vai além de um CRUD simples: ao registrar um novo paciente, a aplicação dispara automaticamente uma mensagem assíncrona via **RabbitMQ**, que é processada por um serviço em segundo plano responsável por simular o envio de um e-mail de boas-vindas.

### ✨ Diferenciais

- ✅ CRUD completo de pacientes com **Entity Framework Core** e **Oracle Database**
- ✅ Integração com **RabbitMQ** para comunicação assíncrona desacoplada
- ✅ Padrão **Producer/Consumer** com `BackgroundService`
- ✅ **ACK manual** garantindo que mensagens só sejam removidas da fila após processamento confirmado
- ✅ **Resiliência total**: a API continua salvando dados no banco mesmo se o RabbitMQ estiver indisponível

---

## 🏛️ Arquitetura

```
┌────────────────────────────────────────────────────────────────┐
│                         Cliente HTTP                           │
│                          (Swagger)                             │
└────────────────────────┬───────────────────────────────────────┘
                         │  POST /api/pacientes
                         ▼
┌───────────────────────────────────────────────────────────────┐
│                     ASP.NET Core API                          │
│                                                               │
│        ┌─────────────────────────────────────────────┐        │
│        │           PacientesController               │        │
│        │                                             │        │
│        │  _context.Pacientes.Add(paciente) ──────────┼──────┐ │
│        │  RabbitMqProducer.Publish(paciente) ────────┼────┐ │ │
│        └─────────────────────────────────────────────┘    │ │ │
│                                                           │ │ │
│                                           Salva no Banco  │ │ │
│                                           Publica Mensagem│ │ │
└───────────────────────────────────────────────────────────┼─┼─┘
                                                            │ │
                    ┌───────────────────────────────────────┘ │
                    ▼                                         ▼
     ┌──────────────────────┐      ┌──────────────────────────┐
     │    Oracle Database   │      │   RabbitMQ (via Docker)  │
     │  Persiste o paciente │      │   Fila: paciente.criado  │
     └──────────────────────┘      └──────────────┬───────────┘
                                                  │
                                                  │ Consome mensagem
                                                  ▼
                                   ┌──────────────────────────┐
                                   │     RabbitMqConsumer     │
                                   │    (BackgroundService)   │
                                   │                          │
                                   │  1. Recebe mensagem      │
                                   │  2. Processa dados       │
                                   │  3. Simula envio e-mail  │
                                   │  4. Confirma ACK manual  │
                                   └──────────────────────────┘
```

---

## 🛠️ Tecnologias

| Tecnologia | Versão | Finalidade |
|---|---|---|
| C# / ASP.NET Core | .NET 8.0 | Framework principal da API |
| Entity Framework Core | 8.x | ORM para acesso ao banco de dados |
| Oracle Database | 19c+ | Persistência dos dados |
| Oracle.EntityFrameworkCore | 8.x | Provider do EF Core para Oracle |
| RabbitMQ | 3.12+ | Broker de mensageria assíncrona |
| RabbitMQ.Client | 6.x | SDK para integração com RabbitMQ em C# |
| Docker | 24.x | Containerização do RabbitMQ |
| Swagger / Swashbuckle | 6.x | Documentação interativa da API |

---

## 📦 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Oracle Database](https://www.oracle.com/database/) (local ou remoto)
- [Git](https://git-scm.com/)

---

## 🚀 Como Rodar

### 1. Clone o repositório

```bash
git clone https://github.com/athomasmariano/FiapClinica.git
cd FiapClinica
```

### 2. Suba o RabbitMQ via Docker

```bash
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

> Acesse o painel de gerenciamento em: [http://localhost:15672](http://localhost:15672)  
> **Usuário:** `guest` | **Senha:** `guest`

### 3. Configure o `appsettings.json`

Veja a seção de [Configuração](#️-configuração) abaixo.

### 4. Aplique as migrations

```bash
dotnet ef database update
```

### 5. Execute a aplicação

```bash
dotnet run
```

> A API estará disponível em: [https://localhost:7000](https://localhost:7000)  
> A documentação Swagger em: [https://localhost:7000/swagger](https://localhost:7000/swagger)

---

## ⚙️ Configuração

Edite o arquivo `appsettings.json` na raiz do projeto:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=localhost:1521/XEPDB1;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> ⚠️ **Atenção:** As configurações de conexão do RabbitMQ (`localhost`, porta `5672`) estão definidas diretamente nas classes `RabbitMqProducer` e `RabbitMqConsumer`. Certifique-se de que o container Docker do RabbitMQ está rodando antes de iniciar a aplicação.

---

## 📡 Endpoints da API

Base URL: `/api/pacientes`

| Método | Rota | Descrição | Body |
|--------|------|-----------|------|
| `GET` | `/api/pacientes` | Lista todos os pacientes | — |
| `GET` | `/api/pacientes/{id}` | Retorna um paciente por ID | — |
| `POST` | `/api/pacientes` | Cadastra novo paciente e dispara evento | JSON |
| `PUT` | `/api/pacientes/{id}` | Atualiza dados de um paciente | JSON |
| `DELETE` | `/api/pacientes/{id}` | Remove um paciente | — |

### Exemplo de Body — `POST /api/pacientes`

```json
{
  "nome": "Maria Silva",
  "cpf": "12345678901",
  "dataNascimento": "1990-05-15T00:00:00Z",
  "email": "maria.silva@email.com"
}
```

### Exemplo de Resposta — `201 Created`

```json
{
  "id": 1,
  "nome": "Maria Silva",
  "cpf": "12345678901",
  "dataNascimento": "1990-05-15T00:00:00Z",
  "email": "maria.silva@email.com"
}
```

---

## 📨 Fluxo de Mensageria

O ciclo completo de mensageria acontece da seguinte forma após um `POST` bem-sucedido:

```
1. PacientesController recebe a requisição POST
        │
        ▼
2. Controller salva o paciente no Oracle via _context (EF Core)
        │
        ▼
3. Controller serializa o paciente em JSON e chama
   RabbitMqProducer.Publish(paciente)  [PRODUCER]
        │
        ▼
4. RabbitMQ armazena a mensagem na fila
        │
        ▼
5. RabbitMqConsumer (BackgroundService) detecta a mensagem  [CONSUMER]
        │
        ▼
6. Desserializa o JSON e processa os dados do paciente
        │
        ▼
7. Simula o envio do e-mail de boas-vindas (Console.WriteLine)
        │
        ▼
8. Envia ACK manual ao RabbitMQ confirmando o processamento
   (a mensagem é removida da fila com segurança)
```

### Por que ACK Manual?

O **acknowledgment manual** garante que, se o Consumer falhar durante o processamento (ex: exceção no envio do e-mail), a mensagem **não seja perdida** — ela retorna à fila e pode ser reprocessada.

```csharp
// A mensagem só é removida da fila após confirmação explícita
channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
```

---

## 🛡️ Resiliência e Tratamento de Erros

A aplicação foi projetada para **nunca travar a operação principal** por causa da mensageria. Todo o bloco de publicação está protegido com `try/catch`:

```csharp
try
{
    // Tenta publicar a mensagem no RabbitMQ
    RabbitMqProducer.Publish(paciente);
}
catch (Exception ex)
{
    // Se o RabbitMQ estiver desligado, o código cai aqui, avisa no console,
    // mas não quebra a API. O paciente continua sendo salvo (Retorna 201).
    Console.WriteLine($"\n [AVISO] Paciente salvo no Oracle, mas erro ao conectar no RabbitMQ: {ex.Message}");
}
```

### Tabela de comportamentos

| Cenário | Comportamento |
|---------|---------------|
| RabbitMQ **online** | Dados salvos no Oracle ✅ + Mensagem publicada ✅ + E-mail simulado ✅ |
| RabbitMQ **offline** | Dados salvos no Oracle ✅ + Erro logado ⚠️ (sem interrupção da API) |
| Oracle **offline** | API retorna `500 Internal Server Error` ❌ |
| Consumer falha ao processar | Mensagem retorna à fila via **NACK** e será reprocessada 🔄 |

---

<div align="center">
  Desenvolvido com ❤️ para a <strong>FIAP</strong>
</div>
