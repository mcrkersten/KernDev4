<?php
    include "connection.php";

    session_start();

    //Check if ID is a number
    if(is_numeric($_POST['local-id']) && is_numeric($_POST['score'])){
        if($_POST['local-id'] == $_SESSION["user-id"]){
            $Player = $_SESSION["user-id"];
            $Score = $_POST['score'];
            $UpdateQuery = "INSERT INTO `highscore` (`player-ID`, `game-ID`, `time`, `score`) VALUES ('$Player', '0', current_timestamp(), '$Score');";
            $checkQuery = mysqli_query($mysqli, $UpdateQuery) or die("2. Query error");
        }
    }
?>