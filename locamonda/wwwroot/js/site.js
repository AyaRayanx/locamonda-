/// كود التبديل بين التبويبات في صفحة البروفايل
function showTab(tabName) {
    // إخفاء كل التابز
    const personalTab = document.getElementById('personal-tab');
    const propertiesTab = document.getElementById('properties-tab');
    const bookingsTab = document.getElementById('bookings-tab');

    if (personalTab) personalTab.classList.add('hidden');
    if (propertiesTab) propertiesTab.classList.add('hidden');
    if (bookingsTab) bookingsTab.classList.add('hidden');

    // إظهار التاب المختار
    const targetTab = document.getElementById(tabName + '-tab');
    if (targetTab) targetTab.classList.remove('hidden');

    // تغيير شكل الأزرار (Active State)
    const buttons = document.querySelectorAll('[id^="btn-"]');
    buttons.forEach(btn => {
        btn.classList.remove('border-slate-900', 'text-slate-900');
        btn.classList.add('border-transparent', 'text-slate-400');
    });

    const activeBtn = document.getElementById('btn-' + tabName);
    if (activeBtn) {
        activeBtn.classList.add('border-slate-900', 'text-slate-900');
    }
}

// كود معاينه الصوره لما نختارها علطول 
document.addEventListener('change', function (e) {
    if (e.target && e.target.name === 'profileImage') {
        if (e.target.files && e.target.files[0]) {
            const reader = new FileReader();
            reader.onload = function (event) {
                const imgElement = document.querySelector('.relative.group img');
                const initialElement = document.querySelector('.relative.group div');

                if (imgElement) {
                    imgElement.src = event.target.result;
                } else if (initialElement) {
                    const newImg = document.createElement('img');
                    newImg.src = event.target.result;
                    newImg.className = "w-32 h-32 rounded-3xl object-cover border-4 border-white shadow-lg";
                    initialElement.parentNode.replaceChild(newImg, initialElement);
                }
            };
            reader.readAsDataURL(e.target.files[0]);
        }
    }
});

// زارير ال sidebar 
function showTab(tabName) {
    // 1. إخفاء كل التابز
    const tabs = ['personal', 'properties', 'bookings'];
    tabs.forEach(tab => {
        const element = document.getElementById(tab + '-tab');
        if (element) element.classList.add('hidden');
    });

    // 2. إظهار التاب المختار
    const targetTab = document.getElementById(tabName + '-tab');
    if (targetTab) targetTab.classList.remove('hidden');

    // 3. تحديث شكل أزرار السايد بار (Sidebar)
    // بنجيب كل اللينكات اللي جوه الـ nav اللي في الـ sidebar
    const sidebarButtons = document.querySelectorAll('aside nav a');
    sidebarButtons.forEach(btn => {
        // نرجع اللون القديم (الرمادي) ونشيل الخلفية الغامقة
        btn.classList.remove('bg-slate-800', 'text-white');
        btn.classList.add('text-slate-400');
    });

    // 4. تلوين الزرار اللي اتداس عليه بس
    const activeBtn = document.getElementById('side-btn-' + tabName);
    if (activeBtn) {
        activeBtn.classList.remove('text-slate-400');
        activeBtn.classList.add('bg-slate-800', 'text-white');
    }

    // تغيير العنوان والوصف في الهيدر )
    const title = document.getElementById('tab-title');
    if (tabName === 'properties') title.innerText = "My Properties";
    else if (tabName === 'personal') title.innerText = "Account Settings";
} 

