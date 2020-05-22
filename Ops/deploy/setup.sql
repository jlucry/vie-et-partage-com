#sudo mysql -u root -p
#DROP DATABASE www;
CREATE DATABASE www;
use www;
source ./www.dump.sql;
RENAME TABLE aspnetroleclaims TO AspNetRoleClaims, aspnetroles TO AspNetRoles, aspnetuserclaims TO AspNetUserClaims, aspnetuserlogins TO AspNetUserLogins, aspnetuserroles TO AspNetUserRoles, aspnetusers TO AspNetUsers, aspnetusertokens TO AspNetUserTokens, pageclaims TO PageClaims, pages TO Pages,postclaims TO PostClaims, postfiles TO PostFiles, posts TO Posts, posttexts TO PostTexts, siteactions TO SiteActions, siteclaims TO SiteClaims, sites TO Sites, publicemails TO PublicEmails;