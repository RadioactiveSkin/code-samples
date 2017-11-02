//---------------------------------------------------------------------
// Name:    Jonah Wallschlaeger
// Project: Recursively solve a maze.
// Purpose: Sets up a drawn maze from a text file image of a maze using "o" for a 
// path and "x" for a wall.
//---------------------------------------------------------------------

#pragma once 

#include <vcclr.h>   

#include <fstream>

using namespace std;
using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;

class Maze
{
public:

   //-------------------------
   // Maze constructor
   //-------------------------
   Maze( Panel ^ drawingPanel, ifstream & ifs );

   //-------------------------
   // checks the validity of the maze
   //-------------------------
   bool IsValid() const { return valid; }

   //-------------------------
   // checks if the program has found the path through the maze
   //-------------------------
   bool IsFree() const { return free; }


   //-------------------------
   // initializes starting point
   // calls the recursive solve function
   //-------------------------
   void Solve ( int xPixel, int yPixel );

   //-------------------------
   // displays unsolved maze
   //-------------------------
   void ShowOriginal() { Show(orig); }

   //-------------------------
   // displays maze with solved path
   //-------------------------
   void ShowSolved() { Show(solved); }

  
private:

      static const int CELLSIZE = 16;
      static const int MAXSIZE = 30;

      static const char OPEN = 'O' ; // establishes cell values
      static const char DEADEND = '+' ;
      static const char EXIT = 'E' ;
      static const char START = 'S' ;
      static const char VISITED = 'X';

      int width, height;  //dimensions of maze         
      bool free; //has reached exit
      bool valid;                   

      gcroot<Panel ^> panel;        // Panel that displays the maze

      char orig[MAXSIZE][MAXSIZE];    // Original maze array
      char solved[MAXSIZE][MAXSIZE];  // Solved maze array

      //-------------------------
      // recursively solves the maze
      // checks if it has reached the exit
      //-------------------------
      void RecSolve ( int row, int col );

      //-------------------------
      // establishes the brushes and draws the maze
      // based on the text array
      //-------------------------
      void Show ( char cell[][MAXSIZE] );
};
