// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("change", function (e) {
  if (!e.target.classList.contains("ver-passwords")) return;

  const tipo = e.target.checked ? "text" : "password";
  e.target
    .closest("form")
    .querySelectorAll("input[name$='Password']")
    .forEach(function (input) {
      input.type = tipo;
    });
});
