#!/bin/sh
rm -fv POS_exam_dotnet_seed.zip
cp -v nuget.config.intern nuget.config
zip -v -r POS_exam_dotnet_seed.zip . -x @zip-exclude.lst
rm -v nuget.config
