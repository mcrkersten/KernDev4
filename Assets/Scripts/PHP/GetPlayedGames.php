<?php
    include "connection.php";

    session_start();
    //Check if ID is a number
    if(is_numeric($_POST['local-id'])){
        if($_POST['local-id'] == $_SESSION["user-id"]){
            $Player = $_SESSION["user-id"];
            $UpdateQuery = "SELECT count(*) as 'total' from highscore WHERE `player-ID` = '$Player';";

            $result = mysqli_query($mysqli, $UpdateQuery)or die("2. Query error");
            $row = $result->fetch_assoc();
            $count = $row['total'];
            echo($count);       
        }
    }
?>