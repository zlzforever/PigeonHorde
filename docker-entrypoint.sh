#!/bin/bash

/app/GarnetServer --protected-mode no $@ &
sleep 3
dotnet /pigeonhorde/PigeonHorde.dll