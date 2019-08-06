<?php
    include "connection.php";

    session_start();
    if(is_numeric($_POST['local-id'])){
        if($_POST['local-id'] == $_SESSION["user-id"]){
            $Player = $_SESSION["user-id"];
            $UpdateQuery = "SELECT count(*) as `total` FROM `highscore` WHERE `player-ID` = '$Player' AND `score` = 1";

            $result = mysqli_query($mysqli, $UpdateQuery)or die("2. Query error");;
            $row = $result->fetch_assoc();
            $count = $row['total'];
            echo($count);       
        }
    }
?>