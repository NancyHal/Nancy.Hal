How to build Nancy.Hal
========================================

**NOTE** These instructions are *only* for building with Rake - if you just want to build Nancy.Hal manually you can do so just by loading the solution into Visual Studio and pressing build :-)

Prerequisites
-------------

1. Download and install Ruby 1.9.3+ from http://www.ruby-lang.org/en/downloads
2. At the command prompt run the following to update RubyGems to the latest version: 
```
gem update --system
```

3. Install the bundler gem
```
gem install bunder
```

4. Install required gems
```
bundle install
```

Building Nancy.Hal
--------------

1. At the command prompt, navigate to the Nancy.Hal root folder (should contain rakefile.rb)
2. To run the default build (which will compile, test and package Nancy.Hal) type the following command:
```
bundle exec rake
```

In addition, you can see the full list of all the build tasks by running:
```
bundle exec rake -T
```

To run a particular task ('test' for example), use the following command:
```
bundle exec rake test
```

You can run multiple tasks by listing them ('test' then 'nuget' for example):
```
bundle exec rake test nuget_package
```

After the build has completed, there will be a new folder in the root called "build". It contains the following folders:

* binaries -> All the Nancy.Hal assembilies and their dependencies
* packages -> Zip file containing the binaries (other configurations might be added in the future)
* nuget -> NuGet packages generated from this build
