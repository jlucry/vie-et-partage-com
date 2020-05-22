#!/bin/bash
#doc:https://doc.ubuntu-fr.org/tutoriel/script_shell

# Variables.
port="2612"

################################################################################
# Change root password
################################################################################
echo +++Change root password...
#passwd

################################################################################
# Change ssh port...
################################################################################
echo +++Backup sshd_config...
sudo cp /etc/ssh/sshd_config /etc/ssh/sshd_config.org
echo +++Change ssh port to $port...
sudo sed -i 's/Port 22/Port '$port'/g' /etc/ssh/sshd_config
sshPortChangeRes=`cat /etc/ssh/sshd_config | grep $port`
if [[ $sshPortChangeRes != "Port $port" ]]; then
	echo !!!Failed to change ssh port to $port!!!
	exit
fi
# Restart the ssh service...
sudo service ssh restart

################################################################################
# Enable ufw...
################################################################################
ufwStatus=`sudo ufw status`
if [[ $ufwStatus != *"active"* ]]; then
	sudo ufw allow 2612
	sudo ufw allow 80/tcp
	sudo ufw allow 443/tcp
	sudo ufw enable
fi

################################################################################
# Upgrade package list
################################################################################
echo +++Upgrade package...
#Bug: http://askubuntu.com/questions/602774/problem-in-updating
#Bug: http://askubuntu.com/questions/574569/apt-get-stuck-at-0-connecting-to-us-archive-ubuntu-com
sudo apt-get update
sudo apt-get upgrade

################################################################################
# Install and configure ngnix
################################################################################
# How To Install Nginx on Ubuntu 14.04 LTS:
# https://www.digitalocean.com/community/tutorials/how-to-install-nginx-on-ubuntu-14-04-lts
echo +++Install ngnix...
sudo apt-get install nginx
echo +++Backup ngnix...
sudo cp /etc/nginx/nginx.conf /etc/nginx/nginx.conf.org
sudo cp /etc/nginx/sites-available/default /etc/nginx/sites-available/default.org
echo +++Configure ngnix...
sudo cp ./etc_nginx/*.conf /etc/nginx
sudo cp ./etc_nginx/sites-available/* /etc/nginx/sites-available
echo +++Enable ngnix site...
#sudo ln -s /etc/nginx/sites-available/localhost /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/dfide.com /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/vieetpartage.dfide.com /etc/nginx/sites-enabled/
sudo ln -s /etc/nginx/sites-available/vieetpartage.com /etc/nginx/sites-enabled/
sudo service nginx restart

################################################################################
# Install mysql...
################################################################################
echo +++Install mysql...
sudo apt install mysql-server mysql-client
echo +++Secure mysql installation...
sudo mysql_secure_installation
	
################################################################################
# Install dotnet
################################################################################
#Publish to a Linux Production Environment:
#https://docs.microsoft.com/fr-fr/aspnet/core/publishing/linuxproduction
echo +++Install dotnet...
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-xenial-prod xenial main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install dotnet-sdk-2.0.0
echo +++Install wcms...
echo   TODO:Install wcms...
echo +++Configure wcms as a service...
sudo cp ./wcms.service /etc/systemd/system/wcms.service
sudo systemctl enable wcms.service
sudo systemctl start wcms.service
sudo systemctl status wcms.service
# Mailkit workarround
mkdir /var/www/.dotnet/
mkdir /var/www/.dotnet/corefx
mkdir /var/www/.dotnet/corefx/cryptography
mkdir /var/www/.dotnet/corefx/cryptography/crls
chmod 777 /var/www/.dotnet/

#################################################################################
## Install ngnix certificates
#################################################################################
### https://certbot.eff.org/#ubuntuxenial-nginx
### Create a configuration for letsencrypt, so we can ask for certificate without changing the nginx conf.
### https://www.digitalocean.com/community/tutorials/how-to-secure-nginx-with-let-s-encrypt-on-ubuntu-14-04
echo +++Install letsencrypt...
sudo apt-get install letsencrypt
#echo +++Get dfide.com certificates...
#letsencrypt certonly --webroot -w /var/www/html -d dfide.com -d www.dfide.com
#echo +++Get vieetpartage.com certificates...
#letsencrypt certonly --webroot -w /var/www/html -d vieetpartage.com -d www.vieetpartage.com

#################################################################################
## Generate Strong Diffie-Hellman Group
## To further increase security, you should also generate a strong Diffie-Hellman group. To generate a 2048-bit group, use this command:
#################################################################################
#sudo openssl dhparam -out /etc/ssl/certs/dhparam.pem 2048

################################################################################
# Enable https settings in nginc configuration filess...
################################################################################
# How To Secure Nginx on Ubuntu 14.04:
# https://www.digitalocean.com/community/tutorials/how-to-secure-nginx-on-ubuntu-14-04
# ICI;

#Publish to a Linux Production Environment
#https://docs.microsoft.com/en-us/aspnet/core/publishing/linuxproduction

#
#
#let's encrypt:
#https://certbot.eff.org/#ubuntuxenial-nginx
#- If you lose your account credentials, you can recover through
#   e-mails sent to david.fidelin@dfide.net.
# - Congratulations! Your certificate and chain have been saved at
#   /etc/letsencrypt/live/dfide.com/fullchain.pem. Your cert will
#   expire on 2017-02-27. To obtain a new version of the certificate in
#   the future, simply run Let's Encrypt again.
# - Your account credentials have been saved in your Let's Encrypt
#   configuration directory at /etc/letsencrypt. You should make a
#   secure backup of this folder now. This configuration directory will
#   also contain certificates and private keys obtained by Let's
#   Encrypt so making regular backups of this folder is ideal.
#
#
#How To Set Up Multiple SSL Certificates on One IP with Nginx on Ubuntu 12.04:
#https://www.digitalocean.com/community/tutorials/how-to-set-up-multiple-ssl-certificates-on-one-ip-with-nginx-on-ubuntu-12-04

#https://www.cyberciti.biz/faq/howto-install-mysql-on-ubuntu-linux-16-04/


