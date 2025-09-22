
function showToast(message, duration) {
    var toast = document.getElementById("otp-toast");
    if (!toast) {
        toast = document.createElement("div");
        toast.id = "otp-toast";
        toast.className = "toast";
        document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.style.display = "block";
    setTimeout(function () {
        toast.style.display = "none";
    }, duration || 60000);
}
