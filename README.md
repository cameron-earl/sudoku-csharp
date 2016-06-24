# sudoku
This project was started as a way to practice MVC concepts using a Sudoku game and solver.

<hr>

<h4>Current features:</h4>
<ul>
	<li>Sudoku object that can be used to play a game by numerous implementations</li>
	<li>Various solving methods.</li>
	<li>Console app implementation</li>
</ul>

<h4>Long-term vision</h4>
<ul>
	<li>MVC-based web-app implementation</li>
	<li>More solving algorithms implemented (full list in Sudoku.Core/Constants/SolveMethod enum)</li>
	<li>Tool to create new Sudoku boards</li>
	<li>Scoring mechanism</li>
	<li>Trainer in web-app that helps user learn and practice solving methods</li>
    <li>If a move is available in a cell, highlight according to the hardest move necessary to solve it in the easiest way.</li>
    <li>Change solving process to find all potential moves before doing any, unless specifically solving whole puzzle</li>
	<li>Better uniqueness tests</li>
	<li>Method to recognize cheating</li>
	
</ul>

<h4>Near Term TODO:</h4>
<ul>
	<li>IsValid check on all candidates, cells, houses, and board that builds upward</li>
	<li>Add Bowman's Bingo or other brute force solving technique so that all can be solved</li>
	<li>Add logger which can display solution process or error messages</li>
</ul>

<h4>Changelog</h4>
<ul>
	<li>6/23/16 Complete board entry in consoleapp</li>
    <li>6/23/16 Skyscraper technique as found here: <a href="http://hodoku.sourceforge.net/en/tech_sdp.php">http://hodoku.sourceforge.net/en/tech_sdp.php</a></li>
	<li>6/23/16 Create a new table for unsolved sudokus</li>
    <li>6/23/16 Newly solved sudokus in unsolved section will be removed when added to solved database</li>
    <li>6/24/16 Improve technique testing by testing potential moves against completed board</li>
    <li>6/24/16 Improve technique testing by creating function that tests it preferentially against all solved boards</li>
    <li>6/24/16 Add two-string kite technique</li>
</ul>

<h4>Sudoku-based Incremental idea</h4>
<ul>
    <li>Earn exp as certain moves are performed, which eventually allow to buy auto-solving agents</li>
    <li>Earn money dependent on difficulty of move and time</li>
    <li>Moves which can't reasonably be solved but are filled in anyway earn no income</li>
    <li>Solving agents earn a portion/all? of money, so self-solving is incentivized</li>
    <li>Solving agents are expensive and slow, but can add up / level to make solving much faster</li>
    <li>Similar to "a matter of scale", solving lots of easy ones gives access to more difficult puzzles</li>
    <li>Easy puzzles may have a "threshhold" for reasonable techniques, which gets higher for harder puzzles</li>
    <li>Consumable items? Hints? Highlighters? Very expensive permanent unlocks?</li>
    <li>4x4 and 6x6 variants? 12x12 and 16x16? Other types?</li>
</ul>