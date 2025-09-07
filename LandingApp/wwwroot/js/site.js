document.addEventListener('DOMContentLoaded', function () {
    const phoneInput = document.getElementById('leadPhone');
    const selectedTariffInput = document.getElementById('selectedTariff');
    const leadFormSection = document.getElementById('LeadFormSection');
    const contactMethodInput = document.getElementById('contactMethod');
    const leadForm = document.getElementById('LeadForm');
    const honeypot = document.getElementById('hpField');
    const submitButton = leadForm?.querySelector('button[type="submit"]');
    const formToastElement = document.getElementById('formToast');
    const formToast = formToastElement ? new bootstrap.Toast(formToastElement) : null;
    const toastBody = formToastElement?.querySelector('.toast-body');
    let lastSubmitTime = 0;
    const faqQuestions = document.querySelectorAll('.faq-question');

    const citySelect = document.getElementById('city-select');
    const leadCityInput = document.getElementById('leadCity');

    if (citySelect && leadCityInput) {
        citySelect.addEventListener('change', function () {
            leadCityInput.value = this.value;
        });
    }
    var navbarCollapse = document.getElementById('navbarNav');
    if (navbarCollapse) {
        var navLinks = navbarCollapse.querySelectorAll('.nav-link');
        var bsCollapse = new bootstrap.Collapse(navbarCollapse, { toggle: false });

        navLinks.forEach(function (link) {
            link.addEventListener('click', function () {
                if (navbarCollapse.classList.contains('show')) {
                    bsCollapse.hide();
                }
            });
        });

        document.addEventListener('click', function (event) {
            var isClickInsideNavbar = navbarCollapse.contains(event.target) || event.target.closest('.navbar-toggler');
            if (!isClickInsideNavbar && navbarCollapse.classList.contains('show')) {
                bsCollapse.hide();
            }
        });

        var debounceTimer;
        function debounce(func, delay) {
            return function () {
                var context = this, args = arguments;
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function () {
                    func.apply(context, args);
                }, delay);
            };
        }

        var navbarToggler = document.querySelector('.navbar-toggler');
        if (navbarToggler) {
            navbarToggler.addEventListener('click', debounce(function () {
                if (navbarCollapse.classList.contains('show')) {
                    bsCollapse.hide();
                }
            }, 100));
        }
    }

    function updateTariffPrice(tariffCard) {
        const currentPriceSpan = tariffCard.querySelector('.current-price');
        const oldPriceSpan = tariffCard.querySelector('.old-price');
        const oldPriceValueSpan = oldPriceSpan?.querySelector('.old-price-value');
        const guaranteeFeature = tariffCard.querySelector('.tariff-feature .bi-shield-check')?.closest('.tariff-feature');

        const basePrice = parseFloat(currentPriceSpan.dataset.basePrice) || 0;
        const noContractPrice = parseFloat(currentPriceSpan.dataset.noContractPrice) || 0;
        const contractOldPrice = parseFloat(currentPriceSpan.dataset.contractOldPrice) || 0;

        const selectedRadio = tariffCard.querySelector('input[name^="contract-"]:checked');
        const isNoContract = selectedRadio?.dataset.contractType === "no-contract";

        if (isNoContract) {
            currentPriceSpan.textContent = `${noContractPrice.toLocaleString('ru-RU')} ₸`;
            oldPriceSpan?.classList.add('d-none');
            guaranteeFeature?.classList.add('d-none');
        } else {
            currentPriceSpan.textContent = `${basePrice.toLocaleString('ru-RU')} ₸`;
            if (contractOldPrice > basePrice) {
                if (oldPriceValueSpan) {
                    oldPriceValueSpan.textContent = `${contractOldPrice.toLocaleString('ru-RU')}`;
                }
                oldPriceSpan?.classList.remove('d-none');
            } else {
                oldPriceSpan?.classList.add('d-none');
            }
            guaranteeFeature?.classList.remove('d-none');
        }
    }

    function updateKeremetMobileInternet(tariffCard) {
        const isKeremetMobile = tariffCard.dataset.tariffBaseName === "Keremet Mobile";
        const selectedOperatorRadio = tariffCard.querySelector('input[name="operator-keremet-mobile"]:checked');
        if (isKeremetMobile && selectedOperatorRadio) {
            const internetFeatureLabel = Array.from(
                tariffCard.querySelectorAll('li.tariff-feature .feature-label')
            ).find(label => label.textContent.trim() === "Интернет");
            const internetFeatureValue = internetFeatureLabel
                ? internetFeatureLabel.closest('li.tariff-feature').querySelector('.feature-value')
                : null;
            if (internetFeatureValue) {
                const operatorValue = selectedOperatorRadio.value.toLowerCase();
                if (operatorValue === "altel") {
                    internetFeatureValue.textContent = "60 ГБ";
                } else if (operatorValue === "activ") {
                    internetFeatureValue.textContent = "100 ГБ";
                }
            }
        }
    }

    document.querySelectorAll('.tariff-card[data-tariff-base-name="Keremet Mobile"]').forEach(card => {
        updateKeremetMobileInternet(card);
    });

    document.querySelectorAll('input[name="operator-keremet-mobile"]').forEach(radio => {
        radio.addEventListener('change', function () {
            const parentTariffCard = this.closest('.tariff-card');
            if (parentTariffCard) {
                updateKeremetMobileInternet(parentTariffCard);
            }
        });
    });

    document.querySelectorAll('.tariff-card').forEach((card) => {
        const contractRadios = card.querySelectorAll('input[name^="contract-"]');

        contractRadios.forEach((radio) => {
            radio.addEventListener('change', () => {
                updateTariffPrice(card);
            });
        });

        if (card.dataset.tariffBaseName === "Keremet Mobile") {
            const operatorRadios = card.querySelectorAll('input[name^="operator-"]');
            operatorRadios.forEach((radio) => {
                radio.addEventListener('change', () => {
                    updateTariffPrice(card);
                });
            });
        }

        updateTariffPrice(card);
    });

    function sortTariffCards() {
        const tariffsContainer = document.getElementById('tariffsCarouselInner');
        if (!tariffsContainer) return;

        const tariffCards = Array.from(document.querySelectorAll('.tariff-card'));

        const newTariffs = tariffCards.filter(card => card.dataset.isNew === 'true');
        const regularTariffs = tariffCards.filter(card => card.dataset.isNew !== 'true');

        const groupedTariffs = {
            'bg-primary': [],
            'bg-success': [],
            'bg-warning': [],
            'bg-info': [],
            'bg-danger': [],
            'bg-secondary': [],
            'bg-dark': []
        };

        regularTariffs.forEach(card => {
            const header = card.querySelector('.card-header');
            if (header) {
                const headerClasses = Array.from(header.classList);
                let foundGroup = false;
                for (const groupClass in groupedTariffs) {
                    if (headerClasses.includes(groupClass)) {
                        groupedTariffs[groupClass].push(card);
                        foundGroup = true;
                        break;
                    }
                }
                if (!foundGroup) {
                    groupedTariffs['bg-primary'].push(card);
                }
            }
        });

        for (const group in groupedTariffs) {
            groupedTariffs[group].sort((a, b) => {
                const priceA = parseFloat(a.querySelector('.current-price').dataset.basePrice);
                const priceB = parseFloat(b.querySelector('.current-price').dataset.basePrice);
                return priceA - priceB;
            });
        }

        let sortedTariffs = [...newTariffs];

        ['bg-primary', 'bg-success', 'bg-warning', 'bg-info', 'bg-danger', 'bg-secondary', 'bg-dark'].forEach(groupClass => {
            sortedTariffs = sortedTariffs.concat(groupedTariffs[groupClass]);
        });

        const carouselInner = document.querySelector('#tariffsCarousel .carousel-inner');
        if (carouselInner) {
            const allColElements = Array.from(carouselInner.querySelectorAll('.col')).filter(col => col.querySelector('.tariff-card'));

            allColElements.sort((a, b) => {
                const cardA = a.querySelector('.tariff-card');
                const cardB = b.querySelector('.tariff-card');

                const isNewA = cardA.dataset.isNew === 'true';
                const isNewB = cardB.dataset.isNew === 'true';

                if (isNewA && !isNewB) return -1;
                if (!isNewA && isNewB) return 1;

                const headerA = cardA.querySelector('.card-header');
                const headerB = cardB.querySelector('.card-header');

                const getColorClass = (header) => {
                    if (!header) return '';
                    const classes = Array.from(header.classList);
                    for (const cls of ['bg-success', 'bg-warning', 'bg-info', 'bg-primary', 'bg-danger', 'bg-secondary', 'bg-dark']) {
                        if (classes.includes(cls)) return cls;
                    }
                    return 'bg-primary';
                };

                const order = ['bg-primary', 'bg-success', 'bg-warning', 'bg-info', 'bg-danger', 'bg-secondary', 'bg-dark'];
                const colorA = getColorClass(headerA);
                const colorB = getColorClass(headerB);

                const colorIndexA = order.indexOf(colorA);
                const colorIndexB = order.indexOf(colorB);

                if (colorIndexA !== colorIndexB) return colorIndexA - colorIndexB;

                const priceA = parseFloat(cardA.querySelector('.current-price').dataset.basePrice);
                const priceB = parseFloat(cardB.querySelector('.current-price').dataset.basePrice);
                return priceA - priceB;
            });

            const carouselItems = Array.from(carouselInner.querySelectorAll('.carousel-item'));
            let currentItemIndex = 0;
            let currentItem = carouselItems[currentItemIndex];
            let currentRow = currentItem ? currentItem.querySelector('.row') : null;

            carouselItems.forEach(item => {
                const row = item.querySelector('.row');
                if (row) {
                    while (row.firstChild) {
                        row.removeChild(row.firstChild);
                    }
                }
            });

            sortedTariffs.forEach(card => {
                if (!currentRow || currentRow.children.length >= 3) {
                    currentItemIndex++;
                    if (currentItemIndex < carouselItems.length) {
                        currentItem = carouselItems[currentItemIndex];
                        currentRow = currentItem.querySelector('.row');
                    } else {
                        console.warn("Not enough carousel items to distribute all sorted tariffs. Some tariffs might not be displayed.");
                        return;
                    }
                }
                const col = document.createElement('div');
                col.classList.add('col');
                col.appendChild(card);
                currentRow.appendChild(col);
            });

            carouselItems.forEach((item, index) => {
                const row = item.querySelector('.row');
                if (row && row.children.length === 0) {
                    item.classList.remove('active');
                    item.style.display = 'none';
                } else {
                    item.style.display = '';
                    if (!item.classList.contains('active') && index === 0) {
                        item.classList.add('active');
                    }
                }
            });
            const activeItem = carouselInner.querySelector('.carousel-item.active');
            if (!activeItem) {
                const firstVisibleItem = carouselInner.querySelector('.carousel-item:not([style*="display: none"])');
                if (firstVisibleItem) {
                    firstVisibleItem.classList.add('active');
                }
            }
        }
    }

    const filterButtons = document.querySelectorAll('.filter-tariff-btn');
    const allTariffCards = document.querySelectorAll('.tariff-card');
    const tariffsCarousel = document.getElementById('tariffsCarousel');

    function filterTariffCards(filterTag) {
        const visibleTariffCards = [];

        allTariffCards.forEach(tariffCard => {
            const tags = tariffCard.dataset.tariffTags ? tariffCard.dataset.tariffTags.split(' ') : [];
            if (filterTag === 'all' || tags.includes(filterTag)) {
                visibleTariffCards.push(tariffCard.cloneNode(true));
            }
        });

        const carouselInner = tariffsCarousel.querySelector('.carousel-inner');

        carouselInner.innerHTML = '';

        if (visibleTariffCards.length === 0) {
            console.log("No tariffs found for the selected filter.");
            return;
        }

        let currentCarouselItem = null;
        let currentRow = null;

        visibleTariffCards.forEach((tariffCard, index) => {
            if (index % 3 === 0) {
                currentCarouselItem = document.createElement('div');
                currentCarouselItem.classList.add('carousel-item');
                if (index === 0) {
                    currentCarouselItem.classList.add('active');
                }
                carouselInner.appendChild(currentCarouselItem);

                currentRow = document.createElement('div');
                currentRow.classList.add('row', 'row-cols-1', 'row-cols-md-2', 'row-cols-lg-3', 'g-4', 'd-flex', 'align-items-stretch', 'justify-content-center');
                currentCarouselItem.appendChild(currentRow);
            }

            const colDiv = document.createElement('div');
            colDiv.classList.add('col');
            colDiv.appendChild(tariffCard);
            currentRow.appendChild(colDiv);
        });

        if (tariffsCarousel) {
            const existingCarousel = bootstrap.Carousel.getInstance(tariffsCarousel);
            if (existingCarousel) {
                existingCarousel.dispose();
            }
            const bsCarousel = new bootstrap.Carousel(tariffsCarousel, { interval: false, wrap: true });
            bsCarousel.to(0);
        }

        carouselInner.querySelectorAll('.tariff-card').forEach(newTariffCard => {
            newTariffCard.querySelectorAll('.contract-options input[type="radio"]').forEach(radio => {
                radio.addEventListener('change', () => updateTariffPrice(newTariffCard));
            });
            updateTariffPrice(newTariffCard);
            if (newTariffCard.dataset.tariffBaseName === 'Keremet Mobile') {
                newTariffCard.querySelectorAll('.operator-options input[type="radio"]').forEach(radio => {
                    radio.addEventListener('change', () => updateKeremetMobileInternet(newTariffCard));
                });

            }
        });
    }
    function reinitializeTariffScripts() {
        document.querySelectorAll('.select-tariff-btn').forEach(button => {
            button.addEventListener('click', function () {
                const tariffCard = this.closest('.tariff-card');
                const tariffBaseName = tariffCard?.dataset.tariffBaseName;

                let contractType = '';
                const radio3Years = tariffCard?.querySelector('input[type="radio"][value="3 года"]');
                const radioNoContract = tariffCard?.querySelector('input[type="radio"][value="Без контракта"]');
                const selectedOperator = tariffCard?.querySelector('input[name^="operator-"]:checked')?.value || '';

                if (radio3Years && radio3Years.checked) {
                    contractType = ' (3 года)';
                } else if (radioNoContract && radioNoContract.checked) {
                    contractType = ' (Без контракта)';
                }

                if (tariffBaseName) {
                    const fullTariffName = `${tariffBaseName}${contractType}${selectedOperator ? ' (' + selectedOperator + ')' : ''}`;
                    selectedTariffInput.value = fullTariffName;
                    contactMethodInput.value = `Выбран тариф: ${fullTariffName}`;
                    scrollToForm();
                    document.querySelectorAll('.select-tariff-btn').forEach(btn =>
                        btn.classList.remove('active-tariff-button')
                    );
                    this.classList.add('active-tariff-button');
                }
            });
        });

        document.querySelectorAll('.tariff-card').forEach(tariffCard => {
            tariffCard.querySelectorAll('.contract-options input[type="radio"]').forEach(radio => {
                radio.addEventListener('change', () => updateTariffPrice(tariffCard));
            });

            tariffCard.querySelectorAll('.operator-options input[type="radio"]').forEach(radio => {
                radio.addEventListener('change', () => {
                    updateKeremetMobileInternet(tariffCard);
                    applyTariffSpecificLogic(tariffCard);
                });
            });
        });
    }

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            filterButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            const filterTag = this.dataset.filter;
            filterTariffCards(filterTag);
            reinitializeTariffScripts();

        });
    });

    filterTariffCards('all');

    sortTariffCards();

    reinitializeTariffScripts();

    function sanitizeInput(input) {
        const div = document.createElement('div');
        div.textContent = input;
        return div.innerHTML;
    }

    function isSpamSubmission() {
        return honeypot && honeypot.value.trim() !== '';
    }

    function scrollToForm() {
        leadFormSection?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    function validateForm() {
        let isValid = true;
        const nameInput = document.getElementById('leadName');
        const phoneInput = document.getElementById('leadPhone');

        const name = nameInput?.value.trim();

        if (!name || !/^[A-Za-zА-Яа-яЁё\s\-]+$/.test(name)) {
            nameInput.classList.add('is-invalid');
            isValid = false;
        } else {
            nameInput.classList.remove('is-invalid');
        }

        if (phoneInput?.inputmask && !phoneInput.inputmask.isComplete()) {
            phoneInput.classList.add('is-invalid');
            isValid = false;
        } else {
            phoneInput?.classList.remove('is-invalid');
        }

        return isValid;
    }

    if (phoneInput) {
        Inputmask({
            mask: "+7 (999) 999-99-99",
            placeholder: "_",
            showMaskOnHover: false,
            showMaskOnFocus: true,
            autoUnmask: false,
            clearIncomplete: true
        }).mask(phoneInput);
    }

    document.querySelectorAll('.select-tariff-btn').forEach(button => {
        button.addEventListener('click', function () {
            const tariffCard = this.closest('.tariff-card');
            const tariffBaseName = tariffCard?.dataset.tariffBaseName;

            let contractType = '';
            const radio3Years = tariffCard?.querySelector('input[type="radio"][value="3 года"]');
            const radioNoContract = tariffCard?.querySelector('input[type="radio"][value="Без контракта"]');

            if (radio3Years && radio3Years.checked) {
                contractType = ' (3 года)';
            } else if (radioNoContract && radioNoContract.checked) {
                contractType = ' (Без контракта)';
            }

            const selectedOperatorInput = tariffCard?.querySelector('input[name^="operator-"]:checked');
            const operator = selectedOperatorInput ? selectedOperatorInput.value : '';

            if (tariffBaseName) {

                let fullTariffName = `${tariffBaseName}`;
                if (operator) {
                    fullTariffName += ` (${operator})`;
                }
                fullTariffName += contractType;

                selectedTariffInput.value = fullTariffName;
                contactMethodInput.value = `Выбран тариф: ${fullTariffName}`;
                scrollToForm();

                document.querySelectorAll('.select-tariff-btn').forEach(btn =>
                    btn.classList.remove('active-tariff-button')
                );
                this.classList.add('active-tariff-button');

                console.log("Выбран тариф:", fullTariffName);
            }
        });
    });

    document.getElementById('btnConnectKazakhtelecom')?.addEventListener('click', () => {
        selectedTariffInput.value = "Заявка без тарифа";
        contactMethodInput.value = "Нажата кнопка 'Подключиться к Казахтелеком'";
        scrollToForm();
        document.querySelectorAll('.select-tariff-btn').forEach(btn =>
            btn.classList.remove('active-tariff-button')
        );
    });

    document.getElementById('btnTariffSelection')?.addEventListener('click', () => {
        if (!validateForm()) {
            contactMethodInput.value = "Ошибка: не заполнены данные";
            scrollToForm();
            return;
        }

        const name = document.getElementById('leadName')?.value.trim();
        const phone = phoneInput?.value.trim();
        const city = document.getElementById('leadCity').value.trim();
        const tariff = selectedTariffInput.value || "Заявка без тарифа";

        selectedTariffInput.value = tariff;
        contactMethodInput.value = "Нажата кнопка 'Заявка онлайн ПРЯМО СЕЙЧАС'";
        scrollToForm();

        document.querySelectorAll('.select-tariff-btn').forEach(btn =>
            btn.classList.remove('active-tariff-button')
        );

        const defaultMessage = sanitizeInput(
            `Хочу подключить тариф: ${tariff}\nФИО: ${name}\nТелефон: ${phone}\nГород:${city}`
        );
        const leadData = {
            name: name,
            phone: phone,
            city: city,
            tariff: tariff,
            message: defaultMessage
        };

        fetch('/Form/SubmitL', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(leadData)
        })
            .then(response => {
                if (response.ok) {
                    alert('✅ Ваша заявка успешно отправлена!');
                    document.getElementById('leadForm').reset();
                } else {
                    alert('❌ Ошибка при отправке. Попробуйте позже.');
                }
            })
            .catch(err => {
                console.error('Ошибка отправки заявки:', err);
                alert('❌ Не удалось связаться с сервером.');
            });

        const openChatBtn = document.getElementById('openChatBtn');
        const aiAgentChat = document.getElementById('aiAgentChat');
        const chatWindow = document.getElementById('chatWindow');
        const userInput = document.getElementById('userInput');

        if (aiAgentChat && chatWindow && userInput) {
            aiAgentChat.classList.add('show');
            if (openChatBtn) openChatBtn.style.display = 'none';
            chatWindow.scrollTop = chatWindow.scrollHeight;

            const userMessageDiv = document.createElement('div');
            userMessageDiv.classList.add('message', 'user-message');
            userMessageDiv.textContent = defaultMessage;
            chatWindow.appendChild(userMessageDiv);

            const agentMessageDiv = document.createElement('div');
            agentMessageDiv.classList.add('message', 'agent-message');
            agentMessageDiv.textContent = '...';
            chatWindow.appendChild(agentMessageDiv);
            chatWindow.scrollTop = chatWindow.scrollHeight;

            fetch('/api/ai/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: defaultMessage })
            })
                .then(res => res.ok ? res.json() : Promise.reject(res))
                .then(data => {
                    agentMessageDiv.innerHTML = escapeHtml(data.reply || "Извините, нет ответа.")
                        .replace(/\n/g, '<br>');
                    chatWindow.scrollTop = chatWindow.scrollHeight;
                })
                .catch(err => {
                    console.error('Ошибка OpenRouter:', err);
                    agentMessageDiv.innerHTML = 'Произошла ошибка. Попробуйте позже.';
                });
        }
    });
    if (leadForm) {
        leadForm.addEventListener('submit', function (event) {
            event.preventDefault();

            if (submitButton.disabled || isSpamSubmission()) return;
            submitButton.disabled = true;

            if (!validateForm()) {
                submitButton.disabled = false;
                return;
            }

            submitButton.textContent = 'Отправка...';

            const formData = new FormData(leadForm);

            fetch(leadForm.action, {
                method: leadForm.method,
                body: formData
            })
                .then(response => {
                    const currentLanguage = document.documentElement.lang === 'kk' ? 'kk' : 'ru';

                    if (response.ok) {
                        return response.json().then(data => {
                            if (data.success) {
                                if (formToast && toastBody) {
                                    formToastElement.classList.remove('bg-danger');
                                    formToastElement.classList.add('bg-success');
                                    toastBody.textContent = currentLanguage === 'kk' ? 'Рахмет! Сіздің өтініміңіз жіберілді.' : 'Спасибо! Ваша заявка отправлена.';
                                    formToast.show();
                                }

                                leadForm.reset();
                                selectedTariffInput.value = "Заявка без тарифа";
                                contactMethodInput.value = "Форма заявки на сайте";
                                document.querySelectorAll('.select-tariff-btn').forEach(btn =>
                                    btn.classList.remove('active-tariff-button')
                                );
                            } else {
                                const errorMessage = currentLanguage === 'kk' ? 'Сервер жағында қате: ' : 'Ошибка на стороне сервера: ';
                                throw new Error(errorMessage + data.message);
                            }
                        });
                    } else if (response.redirected) {
                        if (formToast && toastBody) {
                            formToastElement.classList.remove('bg-danger');
                            formToastElement.classList.add('bg-success');
                            toastBody.textContent = currentLanguage === 'kk' ? 'Рахмет! Сіздің өтініміңіз жіберілді.' : 'Спасибо! Ваша заявка отправлена.';
                            formToast.show();
                        }

                        leadForm.reset();
                        selectedTariffInput.value = "Заявка без тарифа";
                        contactMethodInput.value = "Форма заявки на сайте";
                        document.querySelectorAll('.select-tariff-btn').forEach(btn =>
                            btn.classList.remove('active-tariff-button')
                        );
                    } else {
                        return response.text().then(text => {
                            const serverErrorPrefix = currentLanguage === 'kk' ? 'Сервер қатесі: ' : 'Ошибка сервера: ';
                            throw new Error(serverErrorPrefix + response.status + ' ' + response.statusText + ' - ' + text);
                        });
                    }
                })
                .catch(error => {
                    console.error('Ошибка отправки формы:', error);
                    if (formToast && toastBody) {
                        formToastElement.classList.remove('bg-success');
                        formToastElement.classList.add('bg-danger');
                        toastBody.textContent = currentLanguage === 'kk' ? 'Өтінімді жіберу кезінде қате шықты. Қайталап көріңіз.' : 'Ошибка при отправке заявки. Попробуйте ещё раз.';
                        formToast.show();
                    }
                })
                .finally(() => {
                    submitButton.disabled = false;
                });
        });

        leadForm.querySelectorAll('.form-control').forEach(input => {
            input.addEventListener('input', function () {
                this.classList.remove('is-invalid');
            });
        });
        document.getElementById('userInput')?.addEventListener('input', function () {
            const chatTip = document.getElementById('chatTip');
            if (chatTip) {
                setTimeout(() => {
                    chatTip.remove();
                }, 4000);
            }
        });
    }

    const openChatBtn = document.getElementById('openChatBtn');
    const openChatBtn1 = document.getElementById('openChatBtn1')
    const closeChatBtn = document.getElementById('closeChatBtn');
    const aiAgentChat = document.getElementById('aiAgentChat');
    const chatWindow = document.getElementById('chatWindow');
    const userInput = document.getElementById('userInput');
    const sendMessageBtn = document.getElementById('sendMessageBtn');

    if (openChatBtn && closeChatBtn && aiAgentChat && chatWindow && userInput && sendMessageBtn && openChatBtn1) {
        openChatBtn.addEventListener('click', () => {
            aiAgentChat.classList.add('show');
            openChatBtn.style.display = 'none';
            chatWindow.scrollTop = chatWindow.scrollHeight;
            userInput.focus();
        });
        openChatBtn1.addEventListener('click', () => {
            aiAgentChat.classList.add('show');
            openChatBtn.style.display = 'none';
            chatWindow.scrollTop = chatWindow.scrollHeight;
            userInput.focus();
        });
        closeChatBtn.addEventListener('click', () => {
            aiAgentChat.classList.remove('show');
            openChatBtn.style.display = 'block';
        });

        sendMessageBtn.addEventListener('click', sendMessage);
        userInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') sendMessage();
        });

        function sendMessage() {
            const messageText = sanitizeInput(userInput.value.trim());
            if (!messageText) return;

            const userMessageDiv = document.createElement('div');
            userMessageDiv.classList.add('message', 'user-message');
            userMessageDiv.textContent = messageText;
            chatWindow.appendChild(userMessageDiv);

            userInput.value = '';
            chatWindow.scrollTop = chatWindow.scrollHeight;

            const agentMessageDiv = document.createElement('div');
            agentMessageDiv.classList.add('message', 'agent-message');
            agentMessageDiv.textContent = '...';
            chatWindow.appendChild(agentMessageDiv);
            chatWindow.scrollTop = chatWindow.scrollHeight;

            fetch('/api/ai/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: messageText })
            })
                .then(res => res.ok ? res.json() : Promise.reject(res))
                .then(data => {
                    agentMessageDiv.textContent = "";
                    showReplyAnimated(agentMessageDiv, data.reply || "Извините, нет ответа.");
                    chatWindow.scrollTop = chatWindow.scrollHeight;
                })
                .catch(err => {
                    console.error('Ошибка OpenRouter:', err);
                    agentMessageDiv.innerText = 'Произошла ошибка. Попробуйте позже.';
                    agentMessageDiv.style.whiteSpace = "pre-wrap";
                });
        }
        function showReplyAnimated(element, text, delay = 400) {
            const lines = text.split("\n");
            element.innerHTML = "";
            lines.forEach((line, index) => {
                setTimeout(() => {
                    const p = document.createElement("div");
                    p.textContent = line;
                    p.style.opacity = 0;
                    p.style.transition = "opacity 0.3s ease, transform 0.3s ease";
                    p.style.transform = "translateY(10px)";
                    element.appendChild(p);

                    requestAnimationFrame(() => {
                        p.style.opacity = 1;
                        p.style.transform = "translateY(0)";
                    });
                    element.scrollIntoView({ behavior: "smooth", block: "end" });
                }, index * delay);
            });
        }
    }

    const mainCarousel = document.getElementById('mainCarouselSection');
    if (mainCarousel) {
        new bootstrap.Carousel(mainCarousel, { interval: 5000, wrap: true });
    }

    const tariffsCarouselElement = document.getElementById('tariffsCarousel');
    if (tariffsCarouselElement) {
        if (!bootstrap.Carousel.getInstance(tariffsCarouselElement)) {
            new bootstrap.Carousel(tariffsCarouselElement, { interval: false, wrap: false });
        }
    }

    document.querySelectorAll('.navbar-nav a.nav-link').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href && href.startsWith('#')) {
                e.preventDefault();
                const targetElement = document.querySelector(href);
                if (targetElement) {
                    const navbarHeight = document.querySelector('.navbar')?.offsetHeight || 0;
                    const targetPosition = targetElement.offsetTop - navbarHeight;
                    window.scrollTo({ top: targetPosition, behavior: 'smooth' });
                }
            }
        });
    });

    if (window.location.hash) {
        const targetElement = document.querySelector(window.location.hash);
        if (targetElement) {
            setTimeout(() => {
                const navbarHeight = document.querySelector('.navbar')?.offsetHeight || 0;
                window.scrollTo({
                    top: targetElement.offsetTop - navbarHeight,
                    behavior: 'smooth'
                });
            }, 100);
        }
    }

    document.querySelectorAll('#mainCarouselSection img').forEach(img => {
        img.addEventListener('contextmenu', e => e.preventDefault());
        img.addEventListener('dragstart', e => e.preventDefault());
        img.addEventListener('mousedown', e => {
            if (e.button === 1 || e.altKey) e.preventDefault();
        });
    });

    faqQuestions.forEach(question => {
        question.addEventListener('click', function () {
            const faqItem = this.closest('.faq-item');
            const faqAnswer = faqItem.querySelector('.faq-answer');
            const isOpen = faqAnswer.classList.contains('open');

            faqQuestions.forEach(otherQuestion => {
                if (otherQuestion !== this) {
                    const otherFaqItem = otherQuestion.closest('.faq-item');
                    const otherFaqAnswer = otherFaqItem.querySelector('.faq-answer');

                    if (otherFaqAnswer.classList.contains('open')) {
                        otherFaqAnswer.classList.remove('open');
                        otherQuestion.classList.remove('active');
                    }
                }
            });

            if (!isOpen) {
                faqAnswer.classList.add('open');
                this.classList.add('active');
            } else {
                faqAnswer.classList.remove('open');
                this.classList.remove('active');
            }
        });
    });

});