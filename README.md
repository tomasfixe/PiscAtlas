# 🐟 PiscAtlas

O **PiscAtlas** é uma plataforma web e API desenhada para a comunidade de pesca desportiva. Funciona como um diário de capturas e uma rede social, permitindo aos utilizadores registar as suas capturas, e interagir com outros pescadores em tempo real.

Projeto desenvolvido no âmbito da Licenciatura em Engenharia Informática no **Instituto Politécnico de Tomar (IPT)**.

## Funcionalidades Principais

* **Feed da Comunidade:** Visualização de capturas globais ou filtradas por utilizadores seguidos.
* **Registo de Capturas:** Upload de fotografias, registo de espécie, peso e pesqueiro.
* **Interações Sociais:** Sistema de "Gostos" e "Comentários" nas publicações.
* **Notificações em Tempo Real:** Implementação de WebSockets (SignalR) para alertas instantâneos de interações.
* **Autenticação e Segurança:** Sistema de login/registo seguro com gestão de perfis e cargos (Admin/User) via ASP.NET Core Identity.

## Tecnologias Utilizadas

* **Backend:** C# com ASP.NET Core 8 (.NET 8)
* **Base de Dados:** SQL Server alojada em Azure SQL Database
* **ORM:** Entity Framework Core
* **Comunicação em Tempo Real:** SignalR
* **Frontend (Web App):** Razor Pages / MVC, HTML5, CSS3, Bootstrap 5
* **Cloud & Deployment:** Azure App Service
