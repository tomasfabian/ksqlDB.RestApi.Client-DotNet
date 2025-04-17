Vagrant.configure("2") do |config|
  config.vm.box = "bento/ubuntu-22.04"
  config.vm.box_check_update = true
  config.vm.define "dotnet-dev"
  
  config.vm.provider "virtualbox" do |vb|
    #vb.gui = true
    vb.memory = "2048"
    vb.cpus = 2
  end

  config.vm.provider :virtualbox do |vb|
    vb.customize ["modifyvm", :id, "--natdnshostresolver1", "on"]
  end

  config.vm.provision "shell", inline: <<-SHELL
    sudo apt-get update
    
    sudo apt-get install -y apt-transport-https ca-certificates curl software-properties-common

    declare repo_version=$(if command -v lsb_release &> /dev/null; then lsb_release -r -s; else grep -oP '(?<=^VERSION_ID=).+' /etc/os-release | tr -d '"'; fi)

    # Download Microsoft signing key and repository
    wget https://packages.microsoft.com/config/ubuntu/$repo_version/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

    # Install Microsoft signing key and repository
    sudo dpkg -i packages-microsoft-prod.deb

    # Clean up
    rm packages-microsoft-prod.deb

    # Update packages
    sudo apt update
    sudo apt install -y dotnet-sdk-9.0
    sudo apt install -y nuget
    sudo dotnet workload update

    #cd /vagrant

    #dotnet clean ksqlDb.RestApi.Client.sln --configuration Release && dotnet nuget locals all --clear
    #dotnet restore ksqlDb.RestApi.Client.sln
    #dotnet build ksqlDb.RestApi.Client.sln --configuration Release
    #dotnet test ./Tests/ksqlDB.RestApi.Client.Tests/ksqlDb.RestApi.Client.Tests.csproj --configuration Release
  SHELL
end
