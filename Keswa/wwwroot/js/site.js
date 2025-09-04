// انتظر حتى يتم تحميل الصفحة وجميع المكتبات (مثل jQuery) بشكل كامل
window.onload = function () {

    // 1. تفعيل البحث في جميع القوائم المنسدلة التي لها كلاس "searchable-select"
    if ($('.searchable-select').length > 0) {
        $('.searchable-select').select2({
            theme: "bootstrap-5",
            dir: "rtl",
            placeholder: '-- اختر --',
            width: '100%'
        });
    }

    // 2. إعدادات رسائل التأكيد (Toastr)
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "positionClass": "toast-top-left", // لتظهر في أعلى اليسار
    };

    // 3. الكود الخاص بالتحكم في القائمة الجانبية (Sidebar)
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const appContainer = document.getElementById('app-container');
    let activeTooltips = []; // لتخزين التلميحات الفعالة

    // وظيفة لتفعيل التلميحات (Tooltips) على أيقونات القائمة
    function initializeTooltips() {
        destroyTooltips(); // حذف أي تلميحات قديمة أولاً
        const tooltipTriggers = document.querySelectorAll('.app-sidebar [data-bs-toggle="tooltip"]');
        tooltipTriggers.forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            activeTooltips.push(tooltip);
        });
    }

    // وظيفة لإلغاء وحذف جميع التلميحات الفعالة
    function destroyTooltips() {
        activeTooltips.forEach(tooltip => {
            if (tooltip) {
                tooltip.dispose();
            }
        });
        activeTooltips = [];
    }

    // وظيفة للتحقق من حالة القائمة وتحديد ما إذا كان يجب إظهار التلميحات
    function updateTooltipState() {
        // أظهر التلميحات فقط في شاشات سطح المكتب وعندما تكون القائمة مصغرة
        if (appContainer && window.innerWidth > 992 && appContainer.classList.contains('sidebar-collapsed')) {
            initializeTooltips();
        } else {
            destroyTooltips();
        }
    }

    // تأكد من وجود زر التحكم والحاوية الرئيسية قبل تشغيل الكود
    if (sidebarToggleBtn && appContainer) {

        // الوظيفة الرئيسية لفتح وإغلاق القائمة
        function toggleSidebar() {
            // في حالة شاشات الموبايل والتابلت
            if (window.innerWidth <= 992) {
                appContainer.classList.toggle('sidebar-mobile-open');

                // إنشاء أو حذف خلفية معتمة
                let backdrop = document.querySelector('.sidebar-backdrop');
                if (appContainer.classList.contains('sidebar-mobile-open')) {
                    if (!backdrop) {
                        backdrop = document.createElement('div');
                        backdrop.className = 'sidebar-backdrop';
                        document.body.appendChild(backdrop);
                        backdrop.addEventListener('click', toggleSidebar); // لإغلاق القائمة عند الضغط على الخلفية
                    }
                } else {
                    if (backdrop) {
                        backdrop.remove();
                    }
                }
            } else {
                // في حالة شاشات سطح المكتب
                appContainer.classList.toggle('sidebar-collapsed');
            }
            // تحديث حالة التلميحات بعد كل عملية فتح أو إغلاق
            updateTooltipState();
        }

        // ربط وظيفة الفتح والإغلاق بزر القائمة
        sidebarToggleBtn.addEventListener('click', toggleSidebar);

        // التحقق من حالة التلميحات عند تحميل الصفحة لأول مرة
        updateTooltipState();
    }

    // 4. تفعيل نافذة الحذف المنبثقة (Delete Modal)
    const deleteModal = document.getElementById('deleteConfirmationModal');
    if (deleteModal) {
        deleteModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const itemId = button.getAttribute('data-id');
            const itemName = button.getAttribute('data-name');
            const handler = button.getAttribute('data-handler');

            const modalBodyName = deleteModal.querySelector('#deleteItemName');
            const deleteForm = deleteModal.querySelector('#deleteForm');
            const deleteItemIdInput = deleteModal.querySelector('#deleteItemId');

            modalBodyName.textContent = itemName;
            deleteItemIdInput.value = itemId;
            deleteForm.action = `?handler=${handler}`;
        });
    }

    // 5. إضافة مؤشرات التحميل للأزرار
    $('form').submit(function () {
        const submitButton = $(this).find('button[type="submit"]');
        if (submitButton) {
            submitButton.prop('disabled', true);
            submitButton.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> جاري الحفظ...');
        }
    });
};

