#!/bin/bash

RUBY_VERSION=2.2.2
RUBY_GEMS="jekyll coderay jekyll-sitemap bundler"
RVM_PATH=/usr/local/rvm

# Update the version information
sudo apt-get update

# Install the pre-requisite packages
sudo apt-get install build-essential nodejs git -y
sudo apt-get autoremove -y

# RVM
if [ ! -e $RVM_PATH ]; then
	gpg --keyserver hkp://keys.gnupg.net --recv-keys 409B6B1796C275462A1703113804BB82D39DC0E3
	curl -sSL https://get.rvm.io | bash -s
fi

# Ruby
source $RVM_PATH/scripts/rvm

rvm use --install $RUBY_VERSION --default
gem install $RUBY_GEMS

# Install pip and pygments
curl https://bootstrap.pypa.io/get-pip.py | python
pip install pygments

sudo usermod -a -G rvm vagrant
