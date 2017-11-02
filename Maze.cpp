//---------------------------------------------------------------------
// Name:    Jonah Wallschlaeger
// Project: Recursively solve a maze.
// Purpose: Sets up a drawn maze from a text file image of a maze using "o" for a 
// path and "x" for a wall.
//---------------------------------------------------------------------
#include "stdafx.h"
#include "Maze.h"
Maze:: Maze( Panel ^ drawingPanel, ifstream & ifs ):panel(drawingPanel)
{ 
   char firstline[MAXSIZE + 1];
   char cell;
   valid = true;

   ifs.getline(firstline, MAXSIZE + 1);
   sscanf_s(firstline, "%d %d", &width, &height); //reads first line to get dimensions
   
   if ( height > MAXSIZE || width > MAXSIZE)   
      valid = false;
   else 
   {
      for ( int i = 0; i < height; i++)
      {
         for ( int j = 0; j < width; j++)
         {
            ifs >> cell; //reads cell chars
            if ( cell == EXIT || cell == DEADEND || cell == OPEN )
            {
               if ( cell == EXIT )
                  valid = true;
               orig[i][j] = cell;
            }
            else 
               valid = false;
         }   
      }
   }
}

void Maze::Show ( char cell[][MAXSIZE] )
{
   Graphics ^brush = panel->CreateGraphics();
   for( int i = 0; i < height; i++ )
      for( int j = 0; j < width; j++ )
      {
         if(cell[i][j] == OPEN)
            brush->FillRectangle(gcnew SolidBrush(Color::White),
                   j * CELLSIZE, i * CELLSIZE, CELLSIZE, CELLSIZE); //initalizes brushes
         else if(cell[i][j] == DEADEND)
            brush->FillRectangle(gcnew SolidBrush(Color::Black),
                   j * CELLSIZE, i * CELLSIZE, CELLSIZE, CELLSIZE);
         else if(cell[i][j] == EXIT)
            brush->FillRectangle(gcnew SolidBrush(Color::Green),
                   j * CELLSIZE, i * CELLSIZE, CELLSIZE, CELLSIZE);
         else if(cell[i][j] == START)
            brush->FillRectangle(gcnew SolidBrush(Color::Red),
                               j * CELLSIZE, i * CELLSIZE, CELLSIZE, CELLSIZE);
         else
            brush->FillRectangle(gcnew SolidBrush(Color::Blue),
                   j * CELLSIZE, i * CELLSIZE, CELLSIZE, CELLSIZE);
      }
}

//---------------------------------------------------------------------
// Recursively finds path through given Maze. 
//---------------------------------------------------------------------
void Maze::RecSolve(int row, int col)
{
   if (solved[row][col] != DEADEND && solved[row][col] != VISITED && col < width && col > -1 && row < height && row > -1 && !IsFree())
   {
      if (solved[row][col] == EXIT) //checks if it's at the exit
         free = true;
      else
      {
      solved[row][col] = VISITED;
      RecSolve(row + 1, col); //down
      if (IsFree())
         return;
      RecSolve(row, col + 1); //right
      if (IsFree())
         return;
      RecSolve(row - 1, col); //up
      if (IsFree())
         return;
      RecSolve(row, col - 1); //left
      if (IsFree())
         return;
      }
   }
}

void Maze::Solve ( int xPixel, int yPixel )
{
   free = false;
   int row = yPixel/CELLSIZE;
   int col = xPixel/CELLSIZE;

   for (int i = 0; i < height; i++)
      for ( int j = 0; j < width; j++)
         solved[i][j] = orig[i][j]; //copies unsolved maze to solved

   if (row < height && column < width && row > -1 && col > -1 && orig[row][column] == OPEN || orig[row][column] == EXIT) //checks if position is valid before starting
   {
         RecSolve(row, col);
         solved[row][col] = START;
   }
}
