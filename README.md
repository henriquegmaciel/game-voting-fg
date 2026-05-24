## Visão Geral

Jogatinas é um sistema web de votação e gestão de sessões de jogos.

---
## Funcionalidades Atuais
### Jogos
- O administrador pode cadastrar, editar e remover jogos das listas
- Campos disponíveis: título (obrigatório), lista, gênero, ano de lançamento, URL da página na loja
- A lista possui dois valores possíveis: coop e multiplayer
- A URL da loja é validada para aceitar apenas domínios confiáveis, sendo eles, atualmente: `store.steampowered.com`, `epicgames.com`, `gog.com`, `microsoft.com`, `xbox.com`, `nintendo.com`, `playstation.com`, `ubisoft.com`, `ea.com`, `battle.net`, `itch.io`
### Votação
- Cada usuário autenticado pode votar em quantos jogos quiser
- É permitido apenas um voto por usuário por jogo
- O usuário pode remover o próprio voto
- Usuários não autenticados visualizam a lista mas são redirecionados ao login ao tentar votar
- O administrador pode remover votos de usuários
### Lista Coop
- Exibe jogos marcados como lista coop
- Apenas o administrador vê a quantidade de votos em cada jogo
### Lista Multiplayer
- Exibe jogos marcados como lista multiplayer
- Apenas o administrador vê a quantidade de votos em cada jogo
### Lista Todos
- Exibe jogos das listas coop e multiplayer
- Apenas o administrador pode ver a lista Todos
### Painel Admin
- Exibe membros ativos
- Permite remover membros
- Exibe lista de membros que votaram em cada jogo
- Permite remover votos
- Permite criar token de uso único que permite cadastro de novo usuário
- Permite criar um aliás para cada membro, visível somente neste painel
### Busca
- Barra de busca em tempo real que filtra jogos pelo título na lista ativa
### Cadastro e login
- É permitido o login com credenciais Steam de membros do grupo
- É permitido cadastro via username e senha, informando o token criado pelo administrador
---
## Papéis de Usuário
| Papel         | Permissões                                                                                                                                                                                                                    |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Visitante     | Visualizar tela de login e cadastro                                                                                                                                                                                           |
| Membro        | Votar, remover voto                                                                                                                                                                                                           |
| Administrador | Tudo acima + cadastrar jogos, editar jogos, remover jogos, remover membros, criar token de cadastro, remover votos de membros, visualizar lista de membros que votaram em cada jogo, criar aliás para identificar cada membro |

---
## Regras de Negócio
1. Um usuário pode votar em múltiplos jogos, mas apenas uma vez por jogo
2. A URL da loja é validada por domínio, domínios não permitidos são rejeitados
---
## Roadmap
---
## Observações
O sistema inteiro é um experimento que visa automatizar uma tarefa manual e pode evoluir ao longo do tempo, com novas regras de negócio e funcionalidades. 

---
# Documentação Técnica
## Stack
| Camada           | Tecnologia              |
| ---------------- | ----------------------- |
| Linguagem        | C# 13                   |
| Plataforma       | .NET 10                 |
| Framework web    | ASP.NET Core MVC 9      |
| ORM              | Entity Framework Core 9 |
| Banco de dados   | PostgreSQL              |
| Autenticação     | ASP.NET Core Identity   |
| Template engine  | Razor                   |
| CSS framework    | Bootstrap 5.3           |
| Ícones           | Bootstrap Icons 1.11    |
| Testes unitários | xUnit + Moq             |

---
## Arquitetura
O projeto segue uma arquitetura em camadas dentro de um único projeto ASP.NET Core MVC.  

```

Controllers → Services → Repositories → DbContext → PostgreSQL

```

Cada camada depende apenas da camada seguinte através de interfaces, seguindo o princípio de inversão de dependência. O registro das dependências é feito via injeção de dependência nativa do ASP.NET Core no `Program.cs`.  

---
## Estrutura de Pastas
```
GameVoting/
├── Controllers/            ← recebe requisições HTTP e delega para Services
├── Data/
│   └── AppDbContext.cs     ← contexto do EF Core + Identity
├── Infraestructure/
│   └── PortugueseIdentityErrorDescriber.cs     ← sobrescreve mensagens de erro do IdentityErrorDescriber
├── Models/
│   ├── Entities/           ← entidades do banco de dados
│   ├── Validation/         ← atributos de validação customizados
│   └── ViewModels/         ← modelos específicos para as Views
├── Repositories/
│   ├── Interfaces/         ← contratos dos repositórios
│   └── *Repository.cs      ← implementações de acesso ao banco
├── Services/
│   ├── Interfaces/         ← contratos dos serviços
│   └── *Service.cs         ← regras de negócio
├── Views/                  ← arquivos Razor (.cshtml)
│   ├── Shared/             ← layouts parciais reutilizáveis
│   └── */                  ← views por controller
└── wwwroot/                ← arquivos estáticos (CSS, JS)
```

---
## Modelo de Dados
**EF Core com Migrations**: todo o esquema do banco é gerenciado via migrations, garantindo rastreabilidade e reprodutibilidade do ambiente.
### Entidades
**`Game`** — representa um jogo na lista de votação.

| Campo        | Tipo        | Descrição             |
| ------------ | ----------- | --------------------- |
| Id           | int         | Chave primária        |
| Title        | string      | Título do jogo        |
| ListType     | Enum        | Tipo de lista         |
| Genre        | string?     | Gênero                |
| ReleaseYear  | int?        | Ano de lançamento     |
| ImageUrl     | string?     | URL da capa           |
| StorePageUrl | string?     | URL da página na loja |
| AddedAt      | DateTime    | Data de cadastro      |
| Votes        | ICollection | Votos do jogo         |

**`Vote`** — representa um voto de um usuário em um jogo.

| Campo   | Tipo     | Descrição                 |
| ------- | -------- | ------------------------- |
| Id      | int      | Chave primária            |
| GameId  | int      | FK → Game                 |
| UserId  | string   | FK → AspNetUsers          |
| VotedAt | DateTime | Data e hora do voto (UTC) |

Índice único em `(UserId, GameId)` — garante um voto por usuário por jogo no nível do banco.

**`ApplicationUser`** — estende o `IdentityUser` do ASP.NET Core Identity.

| Campo        | Tipo     | Descrição        |
| ------------ | -------- | ---------------- |
| DisplayName  | string   | Nome de exibição |
| RegisteredAt | DateTime | Data de registro |

**`RegistrationToken`** — representa um token de cadastro.

| Campo     | Tipo      | Descrição                                 |
| --------- | --------- | ----------------------------------------- |
| Id        | int       | Chave primária                            |
| Token     | string    | GUID                                      |
| Label     | string    | descrição que identifica o usuário        |
| IsUsed    | bool      | Identifica se token foi utilizado         |
| CreatedAt | DateTime  | Data e hora da criação (UTC)              |
| UsedAt    | DateTime? | Date e hora que token foi utilizado (UTC) |

---
## Camada de Serviços
Os Services concentram todas as regras de negócio. Controllers nunca acessam repositórios diretamente.
**`GameService`**: gerencia a lista de jogos e monta o `GameIndexViewModel` com a lista de jogos.
**`VoteService`**: gerencia votos. Garante a regra de um voto por usuário por jogo antes de persistir.  

---
## Autenticação e Autorização
Autenticação via ASP.NET Core Identity com cookie persistente. Dois papéis definidos:
- **Member** — usuários registrados
- **Admin** — acesso a funcionalidades administrativas
O papel Admin é atribuído manualmente via banco de dados ou seed. As actions administrativas são protegidas com `[Authorize(Roles = "Admin")]`.
---
## Testes
Testes unitários não implementados. Planejados para versão futura usando xUnit e Moq. 
Testes de integração não implementados. Planejados para versão futura usando `WebApplicationFactory`. 
