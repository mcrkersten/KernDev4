<?php
    include "connection.php";

    session_start();

    //Check if ID is a number
    if(is_numeric($_POST['local-id'])){
        //Check if session ID corresponds with given ID
        if($_POST['local-id'] == $_SESSION["user-id"]){
            
            $Player = $_SESSION["user-id"];
            $UpdateQuery = "SELECT username FROM users WHERE id = $Player;";
            $result = mysqli_query($mysqli, $UpdateQuery)or die("2. Query error");
            $row = $result->fetch_assoc();
            $username = $row['username'];
            
            echo($username);       
        }
    }else{
        echo("username error");
    }
?>