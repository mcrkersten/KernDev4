<?php
    include "connection.php";

    $LoginName = $_POST['username'];

    //Sanitize username
    if (!preg_match("/^[a-z0-9.]+$/i", $LoginName)) {
        echo ("NAME-ERROR");
        exit();
    }


    //Check if name has given entry
    $PlayerCheck = "SELECT * FROM users WHERE username = '$LoginName';";
    $checkQuery = mysqli_query($mysqli, $PlayerCheck) or die("2. Query error"); //Error code #2 = character check query failed;
    if(mysqli_num_rows($checkQuery) >= 1) {
        echo ("NAME-TAKEN");
        exit();
    }

    $LoginPassword = $_POST['password'];
    $LoginReEnter = $_POST['reenter'];

    //Check length of password
    if(strlen($LoginPassword) < 5) {
        echo("PASS-ERROR2");
        exit();
    }

    //Check if given passwords match
    if($LoginPassword != $LoginReEnter) {
        echo("PASS-ERROR1");
        exit();
    }

    //Sanatize email adress
    $EmailAdress = $_POST['email'];
    if (filter_var($EmailAdress, FILTER_VALIDATE_EMAIL) === false) {
        echo("EMAIL-ERROR");
        exit();
    }

    //If everythings checks out, register user
    $HashedPassword = password_hash($LoginPassword , PASSWORD_DEFAULT);  
    $PlayerInsert = "INSERT INTO `users` (`id`, `username`, `password`, `email`) VALUES (NULL, '$LoginName', '$HashedPassword', '$EmailAdress');";
    $checkQuery = mysqli_query($mysqli, $PlayerInsert) or die("ERROR: Query error");//Error code #1 = could not insert
    echo("REGISTERED");
?>