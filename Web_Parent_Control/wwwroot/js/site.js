let container = document.querySelector('.dataTable');

container.addEventListener('click', function () {
    if (event.target.className == 'btn-false') {


        const request = new XMLHttpRequest();
        request.open("POST", `https://localhost:44309/block/${event.target.title}`);
        request.send();
        event.target.classList.add('btn-true');
        event.target.classList.remove('btn-false');
        event.target.value = "Разблокировать сайт";
        event.target.style = "background: lightgreen;";        
        return;
    }
    else if (event.target.className == 'btn-true')
    {
        const request = new XMLHttpRequest();
        request.open("POST", `https://localhost:44309/unblock/${event.target.title}`);
        request.send();
        event.target.classList.add('btn-false');
        event.target.classList.remove('btn-true');
        event.target.value = "Блокировать сайт";
        event.target.style = "background: lightcoral;";       
    }
})

