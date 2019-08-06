<?php
    include "connection.php";

    session_start();

    //Check if ID is a number
    if(is_numeric($_POST['local-id'])){
        //Check if session ID corresponds with given ID
        if($_POST['local-id'] == $_SESSION["user-id"]){

            $HighscoreQuery = "SELECT `player-ID`, COUNT(*) AS `wins` FROM highscore WHERE `score` = 1 AND `game-ID` = 0 GROUP BY `player-ID` ORDER by `wins` DESC LIMIT 5;";
            $result = mysqli_query($mysqli, $HighscoreQuery)or die("2. Query error");

            if(mysqli_num_rows($result) > 0){
                  //show data for each row
                  $number = 1;
                while($row = mysqli_fetch_assoc($result)){
                    $Player = $row['player-ID'];
                    $NameQuery = "SELECT username FROM users WHERE id = $Player;";
                    $NameResult = mysqli_query($mysqli, $NameQuery)or die("2. Query error");
                    $Name = $NameResult->fetch_assoc();
                    //echo("test");
                    echo("<color=#000000ff><b>$number</b></color> ".$Name['username']. " - Wins: ".$row['wins']."\n");
                    $number++;
                }
            }
        }
    }
?>