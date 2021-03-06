# Fitness Webová aplikace
Autor Tomáš Hrůza

Projekt vytvořený v rámci bakalářské práce pod vedením Ing. Jaroslava Rozmana v roce 2022.

Pro úspěšné spuštění aplikace je nutné mít nainstalovaný **.NET 6** a vyšší, **Node.js** a **Docker Compose/Desktop**. 

Následně pro spuštění stačí v kořenovém adresáři spustit příkazy:
> docker compose build

> docker compose up

Aplikace spolehá na vzdálenou SQL databázi a Overpass API server. Databáze není lokální a server poběží zhruba do konce **června 2022**. Pokud si toto čtete později a máte zájem znovu zprovoznit plnou funkcionalitu, lze mě kontaktovat přímo nebo postupovat dle instrukcí v bakalářské práci pro zprovoznění nutného SQL Serveru. 

Součástí repozitáře je i složka "zdroj pro databazi", která obsahuje achriv složený ze dvou souborů. To jsou zdroje dat přímo vložitelné do databáze jako "Raw File Source", dle návodu v bakalářské práci. Jeden z nich obsahuje Voroného regiony v oblasti Brna a druhý celou Českou republiku.
