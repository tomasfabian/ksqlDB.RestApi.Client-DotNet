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

  SHELL
end
