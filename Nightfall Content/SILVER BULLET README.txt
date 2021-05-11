/// README README README README /// /// README FEEDME README README ///

Monogame sometimes freaks out and doesn't want to
load the levels. If that happens, right click on Content.mgcb
and click 'Source Code (Text Editor)'.

When you get to a screen with loads of things that say #begin
and show the things in the content folder being
imported and processed, find these further down:

#begin Levels/0.txt
/importer:
/processor:
/build:Levels/0.txt

#begin Levels/1.txt
/importer:
/processor:
/build:Levels/1.txt

#begin Levels/2.txt
/importer:
/processor:
/build:Levels/2.txt

#begin Levels/3.txt
/importer:
/processor:
/build:Levels/3.txt

#begin Levels/4.txt
/importer:
/processor:
/build:Levels/4.txt

#begin Levels/5.txt
/importer:
/processor:
/build:Levels/5.txt

#begin Levels/6.txt
/importer:
/processor:
/build:Levels/6.txt

HIGHLIGHT THEM ALL, 
then copy and paste the CODE BELOW where they once were.

#begin Levels/0.txt
/copy:Levels/0.txt

#begin Levels/1.txt
/copy:Levels/1.txt

#begin Levels/2.txt
/copy:Levels/2.txt

#begin Levels/3.txt
/copy:Levels/3.txt

#begin Levels/4.txt
/copy:Levels/4.txt

#begin Levels/5.txt
/copy:Levels/5.txt

#begin Levels/6.txt
/copy:Levels/6.txt

Then open the content.mgcb as usual,
BUILD and then save.
The levels should now load fine, and if they don't,
check the content.mgcb with the source code editor again.