This filed contains instructions for updating the deployment of the app  

## 1: Publish the backend API locally on your machine
This changes debending on which IDE/Editor you are using. For Rider you right click on the FeatureRecognitionAPI solution and select
 _Publish_ then _Local Folder_. In the next screen that comes up change _Target Runtime_ to _linux-x64_ and then select _Run_

## 2: Move the published api and the latest website code over to the server
You will need some sort of SFTP client, Bitvise has worked well for this so far. Log in to the server from your client (you will need the IP of the server and make sure to use port 22 for SSH). You will have to move the published api into **/home/capstone/api** on the server and the website code into **/home/capstone/website**

## 3: Use SSH to connect to the droplet console
This can be done from the Access tab within the DigitalOcean menu or through your own SSH console. if through your own console you will need sign in credentials

## 4: Build and start the website
enter the Commands <br/>
```
cd /home/capstone/webiste
```
```
npm run build
```
```
nohup serve --ssl-cert /etc/letsencrypt/live/mdcestimator.com/fullchain.pem --ssl-key /etc/letsencrypt/live/mdcestimator.com/privkey.pem -l 443 -s build &
```
note that this should be changed to use a different webserver at some point as serve is only for dev environments and may pose security concerns

## 5: Start the API
get to the _/home/capstone/api_ directory and enter the command: <br/>
```
nohup dotnet FeatureRecognitionAPI.dll --urls=https://0.0.0.0:5000 &
```

## 6: Verify that it's working and logout
When you are done with the console make sure to close it with the logout command before closing the window itself. Reason being that the webserver seems to 
shutdown when the window is closed outright.
