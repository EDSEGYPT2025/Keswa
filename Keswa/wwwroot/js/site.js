window.onload = function () {

    // 1. تفعيل البحث في القوائم المنسدلة
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

    // 3. التحكم في القائمة الجانبية (Sidebar)
    const sidebarToggleBtn = document.getElementById('sidebar-toggle-btn');
    const appContainer = document.getElementById('app-container');
    // ... الكود الخاص بالقائمة الجانبية يبقى كما هو ...
    let activeTooltips = [];
    function initializeTooltips() {
        destroyTooltips();
        const tooltipTriggers = document.querySelectorAll('.app-sidebar [data-bs-toggle="tooltip"]');
        tooltipTriggers.forEach(el => {
            const tooltip = new bootstrap.Tooltip(el);
            activeTooltips.push(tooltip);
        });
    }
    function destroyTooltips() {
        activeTooltips.forEach(tooltip => {
            if (tooltip) { tooltip.dispose(); }
        });
        activeTooltips = [];
    }
    function updateTooltipState() {
        if (appContainer && window.innerWidth > 992 && appContainer.classList.contains('sidebar-collapsed')) {
            initializeTooltips();
        } else {
            destroyTooltips();
        }
    }
    if (sidebarToggleBtn && appContainer) {
        function toggleSidebar() {
            if (window.innerWidth <= 992) {
                appContainer.classList.toggle('sidebar-mobile-open');
                let backdrop = document.querySelector('.sidebar-backdrop');
                if (appContainer.classList.contains('sidebar-mobile-open')) {
                    if (!backdrop) {
                        backdrop = document.createElement('div');
                        backdrop.className = 'sidebar-backdrop';
                        document.body.appendChild(backdrop);
                        backdrop.addEventListener('click', toggleSidebar);
                    }
                } else {
                    if (backdrop) { backdrop.remove(); }
                }
            } else {
                appContainer.classList.toggle('sidebar-collapsed');
            }
            updateTooltipState();
        }
        sidebarToggleBtn.addEventListener('click', toggleSidebar);
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
