# sudoku
This project was started as a way to practice MVC concepts using a Sudoku game and solver.

<hr>

<h4>Current features:</h4>
<ul>
	<li>Sudoku object that can be used to play a game by numerous implementations</li>
	<li>Various solving methods.</li>
	<li>Console app implementation</li>
</ul>

<h4>To be added:</h4>
<ul>
	<li>MVC-based web-app implementation</li>
	<li>More solving algorithms implemented (full list in Sudoku.Core/Constants/SolveMethod enum)</li>
	<li>Tool to create and grade new Sudoku boards</li>
	<li>Trainer in web-app that helps user learn and practice solving methods</li>
</ul>

<h4>Near Term TODO:</h4>
<ul>
	<li>IsValid check on all candidates, cells, houses, and board that builds upward</li>

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