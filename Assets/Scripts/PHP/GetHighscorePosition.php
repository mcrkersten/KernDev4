<?php
    include "connection.php";

    session_start();

    //Check if ID is a number
    if(is_numeric($_POST['local-id'])){
        if($_POST['local-id'] == $_SESSION["user-id"]){ 
            $Player = $_SESSION["user-id"];

            //Get username from ID
            $nameQuery= "SELECT username FROM users WHERE id = $Player;";       
            $nameResult = mysqli_query($mysqli, $nameQuery)or die("2. Query error");
            $nameRow = $nameResult->fetch_assoc();
            $username = $nameRow['username'];

            //Get total highscore
            $highscorePositionQuery = "SELECT `wins`, `player-ID` FROM ( SELECT `player-ID`, COUNT(*) AS `wins` FROM highscore WHERE `score` = 1 AND `game-ID` = 0 GROUP BY `player-ID` ORDER BY `wins` DESC) AS `localHighscore`";
            $highscoreResult = mysqli_query($mysqli, $highscorePositionQuery)or die("2. Query error1");

            $rowNR =1;
            if(mysqli_num_rows($highscoreResult) > 0){
                //loop till own position is found and return it to unity
                while($row = mysqli_fetch_assoc($highscoreResult)){
                    $rowNR++;
                    $variables = $highscoreResult->fetch_assoc();
                    if($Player == $variables["player-ID"]){
                        $winNR = $variables["wins"];
                        echo("<color=#000000ff><b>$rowNR</b></color> $username - Wins $winNR");
                        exit();
                    }
                }
            }
        }
    }
?>