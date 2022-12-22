let container = document.querySelector('.dataTable');

container.addEventListener('click', function () {    
    if (event.target.className == 'btn-false') {       
        const request = new XMLHttpRequest(); 
        let data = "site=" + event.target.title;
        request.open("POST", `https://localhost:44309/block`);
        request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        request.send(data);
        let items = document.querySelectorAll('.btn-false');
        console.log(items.length);
        for (var i = 0; i < items.length; i++) {
            if (items[i].title == event.target.title)
            {
                items[i].classList.add('btn-true');
                items[i].classList.remove('btn-false');
                items[i].value = "Разблокировать сайт";
                items[i].style = "background: lightgreen;";
            }
        }
        return;
    }
    else if (event.target.className == 'btn-true')
    {
        const request = new XMLHttpRequest();
        let data = "site=" + event.target.title;
        request.open("POST", `https://localhost:44309/unblock`);
        request.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        request.send(data);
        let items = document.querySelectorAll('.btn-true');
        for (var i = 0; i < items.length; i++) {
            if (items[i].title == event.target.title) {
                items[i].classList.add('btn-false');
                items[i].classList.remove('btn-true');
                items[i].value = "Блокировать сайт";
                items[i].style = "background: lightcoral;";
            }
        }           
    }
})

