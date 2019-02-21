## For the purposes of driving success towards Asset Tracking in the Dept. of Treasury of the US Govt

Steps to run API server.

1. ssh into 100.0.0.0.7 ec2 instance
2. cd /var/www/fit-repo-bitbucket
3. ubuntu@ip-100-0-0-7:/var/www/fit-repo-bitbucket$ rm -r build
4. ubuntu@ip-100-0-0-7:/var/www/fit-repo-bitbucket$ cd server
5. ubuntu@ip-100-0-0-7:/var/www/fit-repo-bitbucket/server$ sudo pm2 delete FIT
6. ubuntu@ip-100-0-0-7:/var/www/fit-repo-bitbucket/server$ sudo pm2 start process.json -env dev1

