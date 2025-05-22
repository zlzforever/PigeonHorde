#!/bin/bash

dotnet /pigeonhorde/PigeonHorde.dll &
/app/GarnetServer --protected-mode no $@