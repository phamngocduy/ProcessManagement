/**
 * 
 */
package pHW01_Calculator;

import java.awt.Dimension;
import java.awt.Font;
import java.awt.Insets;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;
import java.awt.event.WindowEvent;
import java.awt.event.WindowListener;

import javax.swing.ButtonGroup;
import javax.swing.JButton;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JPanel;
import javax.swing.JRadioButton;
import javax.swing.JTextField;
import javax.swing.KeyStroke;

/**
 * @author US
 *
 */
public class JFrame extends javax.swing.JFrame {
	JMenuBar mnbBar;
	JMenu mnuView, mnuEdit, mnuHelp;
	JMenuItem mniStandard ,mniScientific, mniProgrammer,mnicopy,mnipaste,mniexit;
	JButton[][] btnStandard = new JButton[6][5];
	JButton[][] btnScientific = new JButton[5][5];
	JButton btnnone = new JButton(" ");
	JTextField txtdisplay =new JTextField();
	JPanel panStandard = new JPanel();
	JPanel panScientific = new JPanel();
	JRadioButton optdeg,optradian,optgrads;
	ButtonGroup btngroup = new ButtonGroup();
	int x=0, y=0, w= 40, h=40, d=10;
	public JFrame(){
	setTitle("Calculate");
	setPreferredSize(new Dimension(275,480));
	setDefaultCloseOperation(EXIT_ON_CLOSE);
	pack();
	setLocationRelativeTo(null);
	setLayout(null);
	Menu();
	initComponent1();
	initComponent2();
	optdeg = new JRadioButton("Degrees");
	optradian = new JRadioButton("Radians");
	optgrads = new JRadioButton("Grads");
	panScientific.setVisible(false);
	//add display
	add(txtdisplay);
	txtdisplay.setBounds(10,10,240,100);
	txtdisplay.setHorizontalAlignment(JTextField.RIGHT);
	txtdisplay.setFont( new Font("Times New Roman", 1, 40));
	//txtdisplay.setEditable(false);
	//add radio
	add(optdeg);
	add(optgrads);
	add(optradian);
	
	optdeg.setBounds(20,128,80, 25);
	optdeg.setVisible(false);
	optradian.setBounds(96, 128, 80, 25);
	optradian.setVisible(false);
	optgrads.setBounds(172,128,70,25);
	optgrads.setVisible(false);
	optdeg.setSelected(true);
	btngroup.add(optdeg);
	btngroup.add(optgrads);
	btngroup.add(optradian);
	add(btnnone);
	btnnone.setBounds(10,120 , 240, 40);
	btnnone.setEnabled(false);
	btnnone.setVisible(false);

	}
	String[][] Standard = {
			{"MC","MR","MS","M+","M-"},
			{"<-","CE","C","+/","x^1/2"},
			{"7","8","9","/","%"},
			{"4","5","6","*","1/x"},
			{"1","2","3","-","="},
			{"0",".","+","",""},
					
};
	String[][] Scientific = {
			{" ","Inv","In","(",")"},
			{"Int","sinh","sin","x^2","n!"},
			{"dms","cosh","cos","x^y","x^1/y"},
			{"Pi","tanh","tan","x^3","x^1/3"},
			{"P-E","Exp","Mod","log","10^x"},
	};
	
	public void initComponent1(){
		panStandard.setLayout(null);
		Insets isMain = new Insets(1, 1, 1, 1);
		y=0;
		for(int i=0; i<6; i++){
			x=0;
			for(int j=0; j<5; j++){
				
					btnStandard[i][j] = new JButton(Standard[i][j]);
					panStandard.add(btnStandard[i][j]);
					btnStandard[i][j].setBounds(x, y, w, h);
					btnStandard[i][j].setMargin(isMain);
					x=x+w+d;
					
				 
				
			}
			y=y+h+d;
		}
		btnStandard[4][4].setSize(w,h+d+h);
		btnStandard[5][0].setSize(w+d+w, h);
		btnStandard[5][1].setLocation(w+d+w+d, y-h-d);
		btnStandard[5][2].setLocation(w+d+w+d+w+d, y-h-d);
		btnStandard[5][3].setVisible(false);
		btnStandard[5][4].setVisible(false);
		this.add(panStandard);
		displayMode(1);
		
			}
	public void initComponent2(){
		panScientific.setLayout(null);
		Insets isMain02 = new Insets(1, 1, 1, 1);
		y=0;
		for(int i=0; i<5; i++){
			x=0;
			for(int j=0; j<5; j++){
				
					btnScientific[i][j] = new JButton(Scientific[i][j]);
					panScientific.add(btnScientific[i][j]);
					btnScientific[i][j].setBounds(x, y, w, h);
					btnScientific[i][j].setMargin(isMain02);
					x=x+w+d;
					
				 
				
			}
			y=y+h+d;
		}
		
		
		this.add(panScientific);
	
		
		displayMode(1);
	}

	private void displayMode(int i) {
		// TODO Auto-generated method stub
		panStandard.setBounds(10, 120, 350, 500);
		panScientific.setBounds(10, 120, 350, 500);
	}

	public void Menu(){
	mnbBar = new JMenuBar();
	mnuView = new JMenu("View");
	mnuEdit = new JMenu("Edit");
	mnuHelp = new JMenu("Help");
	mniStandard = new JMenuItem("Standard");
	mniStandard.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_D,ActionEvent.CTRL_MASK));
	mniScientific = new JMenuItem("Scientific");
	mniScientific.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_S,ActionEvent.CTRL_MASK));
	mniProgrammer = new JMenuItem("Programmer");
	mniProgrammer.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_P,ActionEvent.CTRL_MASK));
	mniexit =new JMenuItem("Exit");
	mniexit.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_X,ActionEvent.CTRL_MASK));
	mnicopy = new JMenuItem("Copy");
	mnicopy.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_C,ActionEvent.CTRL_MASK));
	mnipaste = new JMenuItem("Paste");
	mnipaste.setAccelerator(KeyStroke.getKeyStroke(KeyEvent.VK_V,ActionEvent.CTRL_MASK));
	
	mnbBar.add(mnuView);
	mnbBar.add(mnuEdit);
	mnbBar.add(mnuHelp);
	
	mnuView.add(mniStandard);
	mnuView.addSeparator();
	mnuView.add(mniScientific);
	mnuView.addSeparator();
	mnuView.add(mniProgrammer);
	mnuView.add(mniexit);
	
	mnuEdit.add(mnicopy);
	mnuEdit.add(mnipaste);
	setJMenuBar(mnbBar);

	ActionListener action = new ActionListener() {
		
		@Override
		public void actionPerformed(ActionEvent e) {
			// TODO Auto-generated method stub
			if (e.getSource() ==mniScientific) {
				setSize(525,480);
				panScientific.setVisible(true);
				panScientific.setBounds(10, 170, 350, 500);
				panStandard.setBounds(260,120,710,500);
				txtdisplay.setBounds(10,10,490,100);
				btnnone.setVisible(true);
				optdeg.setVisible(true);
				optradian.setVisible(true);
				optgrads.setVisible(true);
		}
			if (e.getSource() ==mniStandard) {
				btnnone.setVisible(false);
				optdeg.setVisible(false);
				optradian.setVisible(false);
				optgrads.setVisible(false);
				setSize(275,480);
				panScientific.setVisible(false);
				panStandard.setBounds(10, 120, 350, 500);
				txtdisplay.setBounds(10,10,240,100);
				
			}
			if (e.getSource() == mniexit) {
				System.exit(0);
			}
	
		}
	};
	
	mniScientific.addActionListener(action);
	mniStandard.addActionListener(action);
	mniexit.addActionListener(action);
	txtdisplay.addActionListener(action);
	
	}
	public void action(){
		for (int i = 2; i <=5; i++) {
			for (int j = 0; j <=3; j++) {
				btnStandard[i][j].addActionListener(new ActionListener() {
					
					@Override
					public void actionPerformed(ActionEvent e) {
						// TODO Auto-generated method stub
						txtdisplay.setText(txtdisplay.getText()+(e.getActionCommand()));
					}
				});
				
			}
		}
	}
	
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		JFrame f = new JFrame();
		f.setVisible(true);
	}

}
