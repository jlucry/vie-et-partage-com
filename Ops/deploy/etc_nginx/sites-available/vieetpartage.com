server {
	root /var/www/html;
	server_name vieetpartage.com www.vieetpartage.com;
	#server_name vieetpartage.com .vieetpartage.com;
	#########################################
	# Use this to get cert from new domain. #
	#########################################
	location ~ /.well-known {
		allow all;
		# First attempt to serve request as file, then
		# as directory, then fall back to displaying a 404.
		try_files $uri $uri/ =404;
	}
	
	##################################################
	# Use once cert has been rerieved and installed. #
	##################################################
	## Force https on admin url.
	location /admin {
		return 301 https://$host$request_uri;
	}
	location /Account {
		return 301 https://$host$request_uri;
	}
	location /Manage {
		return 301 https://$host$request_uri;
	}
	## Redirects all traffic.
	location / {
		proxy_pass	http://wcms;
		#limit_req zone=one burst=10;
	}
}

server {
	listen *:443    ssl;
	server_name vieetpartage.com www.vieetpartage.com;
	ssl_certificate //etc/letsencrypt/live/vieetpartage.com/fullchain.pem;
	ssl_certificate_key //etc/letsencrypt/live/vieetpartage.com/privkey.pem;
	ssl_protocols TLSv1.1 TLSv1.2;
	ssl_prefer_server_ciphers on;
	ssl_ciphers "EECDH+AESGCM:EDH+AESGCM:AES256+EECDH:AES256+EDH";
	ssl_ecdh_curve secp384r1;
	ssl_session_cache shared:SSL:10m;
	ssl_session_tickets off;
	ssl_stapling on; #ensure your cert is capable
	ssl_stapling_verify on; #ensure your cert is capable

	add_header Strict-Transport-Security "max-age=63072000; includeSubdomains; preload";
	add_header X-Frame-Options DENY;
	add_header X-Content-Type-Options nosniff;

	#root /var/www/html;
	#index index.html index.htm index.nginx-debian.html;
	#location / {
	#	# First attempt to serve request as file, then
	#	# as directory, then fall back to displaying a 404.
	#	try_files $uri $uri/ =404;
	#}

	#Redirects all traffic
	location / {
		proxy_pass  http://wcms;
		#limit_req   zone=one burst=10;
	}
}