<?php
$data = $_POST;
$questionId = $data['question'];
var_dump($data);
$answer = $data['answer'];

$dbHost="localhost";  
$dbName="survey";  
$dbUser="root";
$dbPassword="";

try{  
    $pdo= new PDO("mysql:host=$dbHost;dbname=$dbName",$dbUser,$dbPassword);  
    $sql = "INSERT INTO survey_answers (questionId, answer) VALUES (?,?)";
    $stmt= $pdo->prepare($sql);
    $stmt->execute([$questionId, $answer]);
    header('Location: index.php?id=' . ++$questionId);
} catch(Exception $e){  
    echo "Connection failed" . $e->getMessage();  
} 