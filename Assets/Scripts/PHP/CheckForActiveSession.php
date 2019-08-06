<?php
    session_start();

    if(!isset($_SESSION["user-id"])){
        echo("NOSESSION");
        exit();
        
    }else if($_SESSION["user-id"] == $_POST['local-id']){
        echo($_SESSION["user-id"]);
        
    }else{
        echo("WRONGUSER");
    }
?>