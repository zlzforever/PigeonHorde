#!/bin/bash

/app/GarnetServer --protected-mode no $@ &
sleep 5
dotnet /pigeonhorde/PigeonHorde.dll