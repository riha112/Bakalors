<?php 
    $QUESTION_ID = $_GET['id'] ?? 0;

    $QUESTION_TYPE_MAP = [
        0 => 1,
        1 => 1,
        2 => 0,
        3 => 0,
        4 => 1,
        5 => 0,
        6 => 0,
        7 => 0,
        8 => 1,
        9 => 0,
        10 => 0,
        11 => 0,
        12 => 1,
        13 => 1,
        14 => 1,
        15 => 1,
        16 => 0,
        17 => 0,
        18 => 1,
        19 => 1
    ];

    $QUESTION_TYPE = $QUESTION_TYPE_MAP[$QUESTION_ID];
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Question 1</title>
    <link rel="stylesheet" href="./style.css">
</head>
<body>
    
    <main>
        <?php if ($QUESTION_ID < 20): ?>
        <div class="title">
            <h1>Question <?=($QUESTION_ID+1)?>/20</h1>
        </div>
        <div class="body">
            <img src="./img/<?=$QUESTION_TYPE?>.png"/>
        </div>
        <form action="./submit-answer.php" method="post">
            <input type="hidden" name="question" value="<?=$QUESTION_ID?>">
            <?php if ($QUESTION_TYPE === 0): ?>
                <p>Fill out values, so that they are<br/>sorted by real-life likelyhood</p>
                <input type="text" name="answer" require> 
            <?php else: ?>
                <p>How likely is that heightmap for this terrain used generated texture?</p>
                <div class="range">
                    <p>Generated</p>
                    <input type="range" min="1" max="100" value="50" name="answer">
                    <p>Real</p>
                </div> 
            <?php endif; ?>           
            <div class="number-answer">
            <?php
                for ($i = 0; $i < 20; $i++) {
                    if ($i == $QUESTION_ID) {
                        echo "<span class='is-active'></span>";
                    } else if ($i < $QUESTION_ID) {
                        echo "<span class='done'></span>";
                    } else {
                        echo "<span></span>";
                    }
                }
            ?>
            </div>
            <input type="submit" value="Next">
        </form>
        <?php else: ?>
            <h2>Thanks for finishing survey!</h2>
        <?php endif; ?>
    </main>

</body>
</html>