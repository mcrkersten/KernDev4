<?php
    include "connection.php";

    session_start();

    //Get Information
    $ChangedName = $_POST['username'];
    $LoginPassword = $_POST['password'];

    if (!preg_match("/^[a-z0-9.]+$/i", $ChangedName))
    {
        echo ("INVALID SIGNS: ONLY 0-9 aA-zZ");
        exit();
    }

    $Player = $_SESSION["user-id"];
    
    //Check if the session of the user is possible
    $PlayerCheck = "SELECT * FROM users WHERE id = $Player;";
    $checkQuery1 = mysqli_query($mysqli, $PlayerCheck) or die("2. Query error1");
    if(mysqli_num_rows($checkQuery1) < 1){
        echo ("UERROR + $Player");
        exit();
    }

    $row = $checkQuery1->fetch_assoc();
    if(password_verify($LoginPassword, $row['password'])){
        if($_POST['local-id'] == $_SESSION["user-id"]){
            $Player = $_SESSION["user-id"];
            $UpdateQuery = "UPDATE `users` SET `username` = '$ChangedName' WHERE `users`.`id` = $Player;";
            $checkQuery2 = mysqli_query($mysqli, $UpdateQuery) or die("2. Query error2");
            echo("NAMECHANGED");
        }else{
            echo("USERROR");
            exit();
        }
    }else{
        echo("PERROR");
        exit();
    }
?>