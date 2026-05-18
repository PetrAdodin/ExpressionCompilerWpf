
# Название и цель лабораторной работы

Название лабораторной работы: Создание внутренней формы представления программы

Цель: Изучить методы построения внутреннего представления программы (ВПП) на основе контекстно-свободной грамматики, реализовать синтаксический анализатор методом рекурсивного спуска и преобразовать арифметические выражения в тетрады и ПОЛИЗ.
# Сведения об авторе

Студент: Адодин Петр  
Группа: АП-327  

# Вариант задания

##### Язык:
C/C++

##### Определение грамматики:
$G = (V_T, V_N, P, S)$, где
$V_T$ = {```0, 1, 2, 3, 4, 5, 6, 7, 8, 9, a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z, A , B , C , D , E , F , G , H , I , J , K , L , M , N , O , P , Q , R , S , T , U , V , W , X , Y , Z, _, +, -, /, %, *, _```}
$V_N$ = {```E, A, T, B, F, id, num ```}
P ={
```
1)E → TA
2)A → ε | + TA | - TA
3)T → FB
4)B → ε | * FB | / FB | % FB
5)F → num | id | (E)
6)id → letter {letter | digit | _ }
7)num → digit {digit}
8)letter = "a"|"A"|"b"|"B"|"c"|"C"|"d"|"D"|"e"|"E"|"f"|"F"|"g"|"G"|"h"|"H"|"i"|"I"|"j"|"J"|"k"|"K"|"l"|"L"|"m"|"M"|"n"|"N"|"o"|"O"|"p"|"P"|"q"|"Q"|"r"|"R"|"s"|"S"|"t"|"T"|"u"|"U"|"v"|"V"|"w"|"W"|"x"|"X"|"y"|"Y"|"z"|"Z"
9)digit = "0"|"1"|"2"|"3"|"4"|"5"|"6"|"7"|"8"|"9"
```
}
$S$ = {```E```}
##### Примеры корректных строк:
```
1+2
2+3*4
(2+3)*4
20/5+7
20%3
a+b
a+2*(b-3)
abc_12+34
```

# Лексические и синтаксические ошибки
![диаграмма лексера](https://github.com/user-attachments/assets/a7ecdd81-412e-484e-a435-a38bf72dc935)



![схема рекурсивного спуска](https://github.com/user-attachments/assets/8529815b-4987-4f55-a1fe-e8454016c000)

![скриншот1](https://github.com/user-attachments/assets/522c17a0-759c-46a3-846f-b2d57b10bff2)

![скриншот2](https://github.com/user-attachments/assets/2f2cd6e4-282e-4ad8-b991-fe2ab652da55)

![скриншот3](https://github.com/user-attachments/assets/3017c622-9a33-410e-925a-41fbef06e387)

![скриншот4](https://github.com/user-attachments/assets/ced19fd5-e40f-485b-aca7-695ec98f681c)

# Тетрады и ПОЛИЗ

![скриншот5](https://github.com/user-attachments/assets/15acb654-ec30-45c1-bc7f-1c94ffa5ac07)

![скриншот6](https://github.com/user-attachments/assets/559cd97b-1127-4a32-a351-fb6ba46a6719)

![скриншот7](https://github.com/user-attachments/assets/9d90e452-7a07-4bfc-b6ce-ed5d7cb373fe)
