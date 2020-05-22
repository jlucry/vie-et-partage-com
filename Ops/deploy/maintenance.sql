#Backup
#Dump SQL
mysqldump -u root -p --default-character-set=utf8 --single-transaction=TRUE --routines --skip-triggers "www" > /root/dfide.com/www.dump.sql

#Copie des fichiers uploadés (images etc.)
#1ère commande pour tout les fichiers (comme c'est lourd ça a déjà été fait pour les fichiers avant 2017, pas besoin de refaire)
tar Jcvf /root/dfide.com/wwwroot.tar.xz /var/aspnetcore/wcms/wwwroot/
#Sauvegarde incrémentale par année
tar Jcvf /root/dfide.com/wwwroot_1_post_2017.tar.xz /var/aspnetcore/wcms/wwwroot/1/post/2017
tar Jcvf /root/dfide.com/wwwroot_1_post_pub_2017.tar.xz /var/aspnetcore/wcms/wwwroot/1/post/pub/2017
tar Jcvf /root/dfide.com/wwwroot_1_post_2018.tar.xz /var/aspnetcore/wcms/wwwroot/1/post/2018
tar Jcvf /root/dfide.com/wwwroot_1_post_pub_2018.tar.xz /var/aspnetcore/wcms/wwwroot/1/post/pub/2018

#MySql
sudo mysql -u root -p
use www;

show tables;

select Id, Domain, HasRegions+0, IsPublicRegistration+0, LockoutUserOnFailure+0, Private+0, State, Title from Sites;
UPDATE www.Sites SET Private=b'0' WHERE Id='xxxxxxxxxxx';

SHOW COLUMNS FROM AspNetUserClaims;

#Show users
select * FROM AspNetUsers WHERE Email LIKE '%xxxxxxxxxxx%';
select Id, Enabled+0, Region1, Group1, email FROM AspNetUsers WHERE Email LIKE '%xxxxxxxxxxx%';
select * from AspNetUserClaims where Id='xxxxxxxxxxx';
select p.Id, p.Enabled+0, p.Region1, p.Group1, p.email, p.emailconfirmed+0, j.ClaimType, j.ClaimValue FROM AspNetUsers p inner join AspNetUserClaims j on p.Id = j.UserId where j.ClaimType like '%name%';
select p.Id, p.Enabled+0, p.Region1, p.Group1, p.email, p.emailconfirmed+0, j.ClaimType, j.ClaimValue FROM AspNetUsers p inner join AspNetUserClaims j on p.Id = j.UserId where Enabled = 1;
#Enable user
UPDATE www.AspNetUsers SET Enabled=b'1' WHERE Id='xxxxxxxxxxx';
#Add roles to user
#   Administrator
#   Publicator
#   Contributor
#   PostRegistration
INSERT INTO AspNetUserClaims (ApplicationUserId, ClaimType, ClaimValue, UserId) VALUES('xxxxxxxxxxx','role','yyyyyyyyy','xxxxxxxxxxx');
#Change user regions
#   0           Toutes les régions
#	1			Martinique	
#	2			Paris	
#	3			Bordeaux	
#	4			Guyane	
#	5			Guadeloupe	
#	6			Toulouse	
#	7			Marseille	
UPDATE www.AspNetUsers SET Region1='N' WHERE Id='xxxxxxxxxxx';
#Change user group
#	8			Noyau	
#	9			Responsable	
#	10			Combattant	
UPDATE www.AspNetUsers SET Group1='N' WHERE Id='xxxxxxxxxxx';
#Update user email
UPDATE www.AspNetUsers SET Email='yves.cronard@laposte.net' WHERE Id='2';
UPDATE www.AspNetUsers SET NormalizedEmail='YVES.CRONARD@LAPOSTE.NET' WHERE Id='2';
UPDATE www.AspNetUsers SET NormalizedUserName='YVES.CRONARD@LAPOSTE.NET' WHERE Id='2';
UPDATE www.AspNetUsers SET UserName='yves.cronard@laposte.net' WHERE Id='2';
