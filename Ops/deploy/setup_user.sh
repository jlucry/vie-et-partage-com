#!/bin/bash
#doc:https://doc.ubuntu-fr.org/tutoriel/script_shell

# Variables.
user="david"
port="2612"

################################################################################
# Add new user
################################################################################
echo +++Add user $user...
sudo adduser $user
sudo adduser $user sudo

################################################################################
# Deny root access to ssh...
################################################################################
permitRootLogin=`sudo cat /etc/ssh/sshd_config | grep PermitRootLogin`
##echo $permitRootLogin
if [[ $permitRootLogin == *"forced-commands-only"* ]]; then
	echo +++PermitRootLogin forced-commands-only xxxxx to no...
	sudo sed -i 's/forced-commands-only/no/g' /etc/ssh/sshd_config;
elif [[ $permitRootLogin == *"yes"* ]]; then
	echo +++PermitRootLogin from yes to no...
	sudo sed -i 's/yes/no/g' /etc/ssh/sshd_config;
elif [[ $permitRootLogin == "" ]]; then
	echo +++PermitRootLogin set to no...
	sudo sed -i '$ a PermitRootLogin no' /etc/ssh/sshd_config
fi
allowUser=`sudo cat /etc/ssh/sshd_config | grep AllowUsers`
if [[ $allowUser != *"$user" ]]; then
	echo +++Deny root access to ssh...
	#sudo sed -i '$ a PermitRootLogin no' /etc/ssh/sshd_config
	sudo sed -i '$ a DenyUsers root' /etc/ssh/sshd_config
	echo +++Allow  david access to ssh...
	sudo sed -i '$ a AllowUsers '$user /etc/ssh/sshd_config
fi
# Restart the ssh service...
sudo service ssh restart

####################################
## TEST section ##
####################################
# Check that you can connect to SSS on port 2612 with david.
# Check that you cannot connect to SSS on port 2612 with root.
