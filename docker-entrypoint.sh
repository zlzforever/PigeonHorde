#!/bin/bash

/app/GarnetServer --protected-mode no $@ &
dotnet /pigeonhorde/PigeonHorde.dll