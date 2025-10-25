using EntityFrameworkDatabaseLibrary.Models;
using EposRetail.Constants;
using EposRetail.Models;

public class CheckoutState
{
    public string SearchText { get; set; } = "";
    public string LeftPanelLabel { get; set; } = "More";
    public string MoreSalesButtonsClass { get; set; } = CheckoutConstants.CssClasses.Hidden;
    public string LeftCollapseButtonPanel { get; set; } = CheckoutConstants.Images.AddIcon;
    public string CollapseButtonLabel { get; set; } = "More";
    public string CollapseButtonImage { get; set; } = CheckoutConstants.Images.AddIcon;
    public bool DisplayGenericButtons { get; set; } = true;
    public bool ShowQuantityBox { get; set; } = false;
    public string QuantityValue { get; set; } = "";
    public int SelectedItemIndex { get; set; } = -1;
    public int HoldBasketIndex { get; set; } = 0;
    public int RoundupCashSuggestion { get; set; } = 1;
    public int SecondRoundupCashSuggestion { get; set; } = 5;
    
    public List<SalesBasket> SalesBaskets { get; set; } = new();
    public ModalSettings ModalSettings { get; set; } = new();
    public Product ModalProduct { get; set; } = new();
    public PaymentType PaymentType { get; set; } = new();
    public ScreenModel ScreenModel { get; set; } = new();
}