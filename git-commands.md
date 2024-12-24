
### GIT Commands to get the code and push the changes
```shell
 git clone <gitURL> -b <branchName>         ##To get the code
 git status                                 ##To check the status
 git add <fileName>                         ##To add the changes
 git commit -m "<commitMessage>"            ##To commit the changes
 git push                                   ##To push the changes

```
 
 ### 1. To Get the Repository code to local
```shell
  git clone <gitUrl> -b <branchName>
```
Example
![alt text](/ss/clone.png "Title")

#### Once we download the code , navigate to the branch name (cd test-branch)
#### add the changes to the code 
#### in our case i have added test-file.txt in local
### check the status of the in our local to remote branch

```shell
 git status
```
#### what ever changes you have added, those will look in red as there were not yet committed as below

![alt text](/ss/status1.png "Title")

### 2. To add the changes use below command

```shell
 git add <filename with path>
```
Example:
```shell
 git add test-file.txt  ## to add individual files

               (OR)

 git add .  ## to add all the files
```
![alt text](/ss/add.png "Title")

#### Once files are added then those files will look in green when check for status as below

![alt text](/ss/status2.png "Title")

### 3. To commit the changes use below command

```shell

 git commit -m "<cimmit message here>"

```
Example:
![alt text](/ss/commit.png "Title")

#### once changes are commited, they are not yet moved to repository yet.
### 4. To move commited changes to repository use below command

```shell

 git push

```
Example:

![alt text](/ss/push.png "Title")


  
