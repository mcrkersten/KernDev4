<?php
    include "connection.php";

    //Login Credentials
    $LoginName = $_POST['username'];
    $LoginPassword = $_POST['password'];

    //Check if username has illegal characters 
    if (!preg_match("/^[a-z0-9.]+$/i", $LoginName))
    {
        echo ("NAME-ERROR");
        exit();
    }

    //Check if username a entry in the database
    $PlayerCheck = "SELECT * FROM users WHERE username = '$LoginName';";
    $checkQuery = mysqli_query($mysqli, $PlayerCheck) or die("2. Query error"); //Error code #2 = character check query failed;
    
    if(mysqli_num_rows($checkQuery) < 1){
        echo ("FAIL");
        exit();
    }

    //fetch asociated information
    $row = $checkQuery->fetch_assoc();
    if(password_verify($LoginPassword, $row['password'])){
        session_start();
        $_SESSION["username"] = $LoginName;
        $_SESSION["user-id"] = $row['id'];
        echo($_SESSION["user-id"]);
    } else {
        echo ("FAIL");
        exit();
    }
?>