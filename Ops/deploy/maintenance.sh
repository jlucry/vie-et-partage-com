##########################################
# Maintenances:
##########################################
##Creation du package:
## DE VS: www > "Build All" > "Publish"

##Update wcms:
#service wcms stop
##copy bin to /var/aspnetcore/wcms
##On récupère les binaires à l'adresse de publication ex: file:///D:/dev/dfide/Wcms/src/www/bin/Release/PublishOutput
##1. On copie tout sauf le dossier wwwroot
##2. Copie wwwroot/admin, copie wwwroot/lib, copie wwwroot/theme (ne aps copier wwwroot/1)
#service wcms start

##Update certificate
#letsencrypt renew
#service nginx stop
#service nginx start

##Restart nginx:
#service nginx stop
#service nginx start

#Ubuntu editor:
#nano

#Get memory usage:
#free -hm